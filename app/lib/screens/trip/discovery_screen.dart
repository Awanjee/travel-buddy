import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../models/models.dart';
import '../../providers/providers.dart';

class DiscoveryScreen extends ConsumerStatefulWidget {
  const DiscoveryScreen({super.key, required this.tripId});

  final String tripId;

  @override
  ConsumerState<DiscoveryScreen> createState() => _DiscoveryScreenState();
}

class _DiscoveryScreenState extends ConsumerState<DiscoveryScreen> {
  List<Candidate> _pending = [];
  var _loading = true;
  var _deciding = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final list =
          await ref.read(apiClientProvider).getPendingCandidates(widget.tripId);
      if (mounted) setState(() => _pending = list);
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _decide(String candidateId, String decision) async {
    setState(() => _deciding = true);
    try {
      await ref
          .read(apiClientProvider)
          .decide(widget.tripId, candidateId, decision);
      await _load();
    } finally {
      if (mounted) setState(() => _deciding = false);
    }
  }

  Future<void> _openBooking(String? url) async {
    if (url == null) return;
    final uri = Uri.parse(url);
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }

  @override
  Widget build(BuildContext context) {
    final current = _pending.isNotEmpty ? _pending.first : null;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Discover'),
        actions: [
          TextButton(
            onPressed: () => context.push('/trips/${widget.tripId}/plan'),
            child: const Text('Build plan'),
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : current == null
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(32),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.check_circle_outline, size: 64),
                        const SizedBox(height: 16),
                        const Text(
                          'You\'ve reviewed all suggestions!',
                          textAlign: TextAlign.center,
                          style: TextStyle(
                              fontSize: 18, fontWeight: FontWeight.w600),
                        ),
                        const SizedBox(height: 24),
                        FilledButton(
                          onPressed: () =>
                              context.push('/trips/${widget.tripId}/plan'),
                          child: const Text('Build your itinerary'),
                        ),
                      ],
                    ),
                  ),
                )
              : Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    children: [
                      Align(
                        alignment: Alignment.centerLeft,
                        child: Chip(
                          label: Text(
                            '${_pending.length} left · ${current.tag}',
                          ),
                        ),
                      ),
                      const SizedBox(height: 8),
                      Expanded(
                        child: Card(
                          child: Padding(
                            padding: const EdgeInsets.all(20),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(
                                  children: [
                                    Chip(
                                      label: Text(current.type),
                                      visualDensity: VisualDensity.compact,
                                    ),
                                    const Spacer(),
                                    if (current.priceEstimateUsd != null)
                                      Text(
                                        '\$${current.priceEstimateUsd!.toStringAsFixed(0)} est.',
                                        style: Theme.of(context)
                                            .textTheme
                                            .labelLarge,
                                      ),
                                  ],
                                ),
                                const SizedBox(height: 12),
                                Text(
                                  current.name,
                                  style:
                                      Theme.of(context).textTheme.headlineSmall,
                                ),
                                if (current.location != null) ...[
                                  const SizedBox(height: 4),
                                  Text(current.location!,
                                      style: Theme.of(context)
                                          .textTheme
                                          .labelMedium),
                                ],
                                const SizedBox(height: 16),
                                Expanded(
                                  child: SingleChildScrollView(
                                    child: Text(current.description),
                                  ),
                                ),
                                if (current.bookingUrl != null) ...[
                                  const SizedBox(height: 12),
                                  OutlinedButton.icon(
                                    onPressed: () =>
                                        _openBooking(current.bookingUrl),
                                    icon: const Icon(Icons.open_in_new, size: 18),
                                    label: const Text('Search to book'),
                                  ),
                                ],
                              ],
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),
                      if (_deciding)
                        const CircularProgressIndicator()
                      else
                        Row(
                          children: [
                            Expanded(
                              child: OutlinedButton.icon(
                                onPressed: () =>
                                    _decide(current.id, 'Declined'),
                                icon: const Icon(Icons.close),
                                label: const Text('Skip'),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: FilledButton.icon(
                                onPressed: () =>
                                    _decide(current.id, 'Approved'),
                                icon: const Icon(Icons.favorite_border),
                                label: const Text('Add'),
                              ),
                            ),
                          ],
                        ),
                    ],
                  ),
                ),
    );
  }
}
