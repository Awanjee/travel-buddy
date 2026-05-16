import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../models/models.dart';
import '../../providers/providers.dart';

class ItineraryScreen extends ConsumerStatefulWidget {
  const ItineraryScreen({super.key, required this.tripId});

  final String tripId;

  @override
  ConsumerState<ItineraryScreen> createState() => _ItineraryScreenState();
}

class _ItineraryScreenState extends ConsumerState<ItineraryScreen> {
  Itinerary? _plan;
  var _loading = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadExisting();
  }

  Future<void> _loadExisting() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final plan = await ref.read(apiClientProvider).getItinerary(widget.tripId);
      if (mounted) setState(() => _plan = plan);
    } catch (_) {
      // No plan yet
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _build() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final plan =
          await ref.read(apiClientProvider).buildItinerary(widget.tripId);
      if (mounted) setState(() => _plan = plan);
    } catch (e) {
      if (mounted) {
        setState(() => _error =
            'Approve at least one place, hotel, or activity first.');
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _share() async {
    if (_plan == null) return;
    await SharePlus.instance.share(
      ShareParams(text: _plan!.planMarkdown, subject: 'My Travel Buddy plan'),
    );
  }

  Future<void> _copy() async {
    if (_plan == null) return;
    await Clipboard.setData(ClipboardData(text: _plan!.planMarkdown));
    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Plan copied to clipboard')),
      );
    }
  }

  Future<void> _openPdf() async {
    if (_plan?.exportPdfUrl == null) return;
    final url = ref.read(apiClientProvider).pdfUrl(_plan!.exportPdfUrl!);
    final uri = Uri.parse(url);
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Your plan')),
      body: _loading && _plan == null
          ? const Center(child: CircularProgressIndicator())
          : _plan == null
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(32),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Text(
                          'Build a day-by-day plan from everything you approved.',
                          textAlign: TextAlign.center,
                        ),
                        if (_error != null) ...[
                          const SizedBox(height: 16),
                          Text(
                            _error!,
                            style: TextStyle(
                              color: Theme.of(context).colorScheme.error,
                            ),
                          ),
                        ],
                        const SizedBox(height: 24),
                        FilledButton(
                          onPressed: _build,
                          child: const Text('Generate itinerary'),
                        ),
                      ],
                    ),
                  ),
                )
              : Column(
                  children: [
                    if (_loading)
                      const LinearProgressIndicator(),
                    Expanded(
                      child: SingleChildScrollView(
                        padding: const EdgeInsets.all(16),
                        child: SelectableText(
                          _plan!.planMarkdown,
                          style: const TextStyle(
                            fontFamily: 'monospace',
                            height: 1.4,
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
      bottomNavigationBar: _plan == null
          ? null
          : SafeArea(
              child: Padding(
                padding: const EdgeInsets.all(12),
                child: Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  alignment: WrapAlignment.center,
                  children: [
                    OutlinedButton.icon(
                      onPressed: _share,
                      icon: const Icon(Icons.share),
                      label: const Text('Share'),
                    ),
                    OutlinedButton.icon(
                      onPressed: _copy,
                      icon: const Icon(Icons.copy),
                      label: const Text('Copy'),
                    ),
                    if (_plan!.exportPdfUrl != null)
                      OutlinedButton.icon(
                        onPressed: _openPdf,
                        icon: const Icon(Icons.picture_as_pdf),
                        label: const Text('PDF'),
                      ),
                    FilledButton(
                      onPressed: _build,
                      child: const Text('Regenerate'),
                    ),
                  ],
                ),
              ),
            ),
    );
  }
}
