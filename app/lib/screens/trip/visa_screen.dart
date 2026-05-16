import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../models/models.dart';
import '../../providers/providers.dart';

class VisaScreen extends ConsumerStatefulWidget {
  const VisaScreen({super.key, required this.tripId});

  final String tripId;

  @override
  ConsumerState<VisaScreen> createState() => _VisaScreenState();
}

class _VisaScreenState extends ConsumerState<VisaScreen> {
  VisaGuidance? _visa;
  var _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final visa = await ref.read(apiClientProvider).getVisa(widget.tripId);
      if (mounted) setState(() => _visa = visa);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Could not load visa info: $e')),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _openSource() async {
    final url = Uri.parse(_visa!.sourceUrl);
    if (await canLaunchUrl(url)) await launchUrl(url, mode: LaunchMode.externalApplication);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Visa guidance')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _visa == null
              ? const Center(child: Text('No visa guidance available.'))
              : ListView(
                  padding: const EdgeInsets.all(16),
                  children: [
                    Card(
                      color: Theme.of(context).colorScheme.errorContainer,
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Text(
                          _visa!.disclaimer,
                          style: TextStyle(
                            color: Theme.of(context)
                                .colorScheme
                                .onErrorContainer,
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 16),
                    Text(_visa!.summary,
                        style: Theme.of(context).textTheme.titleMedium),
                    const SizedBox(height: 12),
                    ListTile(
                      leading: const Icon(Icons.schedule),
                      title: Text(
                        'Estimated processing: ${_visa!.timelineMinDays}–${_visa!.timelineMaxDays} days',
                      ),
                      subtitle: Text(_visa!.timelineNotes),
                    ),
                    TextButton.icon(
                      onPressed: _openSource,
                      icon: const Icon(Icons.open_in_new),
                      label: const Text('Official source'),
                    ),
                    const Divider(height: 32),
                    Text('Checklist',
                        style: Theme.of(context).textTheme.titleLarge),
                    const SizedBox(height: 8),
                    ..._visa!.checklist.map(
                      (item) => Card(
                        child: ListTile(
                          leading: Icon(
                            item.isRequired
                                ? Icons.check_circle_outline
                                : Icons.info_outline,
                          ),
                          title: Text(item.title),
                          subtitle: Text(item.description),
                        ),
                      ),
                    ),
                  ],
                ),
      bottomNavigationBar: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: FilledButton(
            onPressed: () => context.push('/trips/${widget.tripId}/discover'),
            child: const Text('Continue to discover places'),
          ),
        ),
      ),
    );
  }
}
