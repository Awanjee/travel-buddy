import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../providers/providers.dart';

class QuestionnaireScreen extends ConsumerStatefulWidget {
  const QuestionnaireScreen({super.key});

  @override
  ConsumerState<QuestionnaireScreen> createState() =>
      _QuestionnaireScreenState();
}

class _QuestionnaireScreenState extends ConsumerState<QuestionnaireScreen> {
  final _pageController = PageController();
  var _page = 0;

  final _destCode = TextEditingController(text: 'JP');
  final _destName = TextEditingController(text: 'Japan');
  final _partySize = TextEditingController(text: '1');
  final _notes = TextEditingController();
  DateTime? _start;
  DateTime? _end;

  String _budget = 'MidRange';
  String _energy = 'Moderate';
  String _visaIntent = 'EmbassyVisa';
  final _prefs = <String>{
    'architecture',
    'history',
    'food',
  };

  var _loading = false;

  @override
  void dispose() {
    _pageController.dispose();
    _destCode.dispose();
    _destName.dispose();
    _partySize.dispose();
    _notes.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() => _loading = true);
    try {
      final trip = await ref.read(apiClientProvider).createTrip({
        'destinationCountryCode': _destCode.text.trim().toUpperCase(),
        'destinationCountryName': _destName.text.trim(),
        'startDate': _start != null
            ? '${_start!.year}-${_start!.month.toString().padLeft(2, '0')}-${_start!.day.toString().padLeft(2, '0')}'
            : null,
        'endDate': _end != null
            ? '${_end!.year}-${_end!.month.toString().padLeft(2, '0')}-${_end!.day.toString().padLeft(2, '0')}'
            : null,
        'partySize': int.tryParse(_partySize.text) ?? 1,
        'budgetBand': _budget,
        'energyLevel': _energy,
        'visaIntent': _visaIntent,
        'preferences': _prefs.toList(),
        'personalNotes':
            _notes.text.trim().isEmpty ? null : _notes.text.trim(),
      });
      if (mounted) {
        context.go('/trips/${trip.id}/visa');
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Could not create trip: $e')),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  void _next() {
    if (_page < 3) {
      _pageController.nextPage(
        duration: const Duration(milliseconds: 300),
        curve: Curves.easeOut,
      );
      setState(() => _page++);
    } else {
      _submit();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Plan your trip'),
        leading: IconButton(
          icon: const Icon(Icons.close),
          onPressed: () => context.pop(),
        ),
      ),
      body: Column(
        children: [
          LinearProgressIndicator(value: (_page + 1) / 4),
          Expanded(
            child: PageView(
              controller: _pageController,
              physics: const NeverScrollableScrollPhysics(),
              children: [
                _pageDestination(),
                _pageStyle(),
                _pageVisa(),
                _pageDetails(),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(16),
            child: FilledButton(
              onPressed: _loading ? null : _next,
              child: _loading
                  ? const SizedBox(
                      height: 22,
                      width: 22,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : Text(_page < 3 ? 'Continue' : 'Create trip'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _pageDestination() {
    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Text('Where are you going?',
            style: Theme.of(context).textTheme.headlineSmall),
        const SizedBox(height: 16),
        TextField(
          controller: _destName,
          decoration: const InputDecoration(labelText: 'Country / destination'),
        ),
        const SizedBox(height: 12),
        TextField(
          controller: _destCode,
          decoration: const InputDecoration(
            labelText: 'Country code (JP, TR, …)',
          ),
          textCapitalization: TextCapitalization.characters,
        ),
        const SizedBox(height: 12),
        TextField(
          controller: _partySize,
          decoration: const InputDecoration(labelText: 'How many travelers?'),
          keyboardType: TextInputType.number,
        ),
      ],
    );
  }

  Widget _pageStyle() {
    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Text('Your travel style',
            style: Theme.of(context).textTheme.headlineSmall),
        const SizedBox(height: 16),
        DropdownButtonFormField<String>(
          value: _budget,
          decoration: const InputDecoration(labelText: 'Budget'),
          items: const [
            DropdownMenuItem(value: 'Budget', child: Text('Budget')),
            DropdownMenuItem(value: 'MidRange', child: Text('Mid-range')),
            DropdownMenuItem(value: 'Luxury', child: Text('Luxury')),
          ],
          onChanged: (v) => setState(() => _budget = v!),
        ),
        const SizedBox(height: 12),
        DropdownButtonFormField<String>(
          value: _energy,
          decoration: const InputDecoration(labelText: 'Energy level'),
          items: const [
            DropdownMenuItem(value: 'Low', child: Text('Relaxed')),
            DropdownMenuItem(value: 'Moderate', child: Text('Moderate')),
            DropdownMenuItem(value: 'High', child: Text('Packed schedule')),
          ],
          onChanged: (v) => setState(() => _energy = v!),
        ),
        const SizedBox(height: 16),
        Text('Interests', style: Theme.of(context).textTheme.titleMedium),
        const SizedBox(height: 8),
        Wrap(
          spacing: 8,
          children: [
            'architecture',
            'history',
            'nightlife',
            'countryside',
            'food',
            'shopping',
          ].map((p) {
            final selected = _prefs.contains(p);
            return FilterChip(
              label: Text(p),
              selected: selected,
              onSelected: (v) {
                setState(() {
                  if (v) {
                    _prefs.add(p);
                  } else {
                    _prefs.remove(p);
                  }
                });
              },
            );
          }).toList(),
        ),
      ],
    );
  }

  Widget _pageVisa() {
    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Text('Visa situation',
            style: Theme.of(context).textTheme.headlineSmall),
        const SizedBox(height: 8),
        const Text(
          'We provide checklists and timelines only — always verify with official sources.',
        ),
        const SizedBox(height: 16),
        ...[
          ('NotSure', 'Not sure yet'),
          ('VisaFree', 'Visa-free / eligible without embassy'),
          ('EVisa', 'Planning e-Visa'),
          ('EmbassyVisa', 'Need embassy / consulate visa'),
          ('AlreadyHaveVisa', 'Already have visa'),
        ].map((e) => RadioListTile<String>(
              title: Text(e.$2),
              value: e.$1,
              groupValue: _visaIntent,
              onChanged: (v) => setState(() => _visaIntent = v!),
            )),
      ],
    );
  }

  Widget _pageDetails() {
    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Text('Dates & notes',
            style: Theme.of(context).textTheme.headlineSmall),
        const SizedBox(height: 16),
        ListTile(
          contentPadding: EdgeInsets.zero,
          title: Text(_start == null
              ? 'Start date (optional)'
              : 'Start: ${_start!.toLocal().toString().split(' ').first}'),
          trailing: const Icon(Icons.calendar_today),
          onTap: () async {
            final d = await showDatePicker(
              context: context,
              firstDate: DateTime.now(),
              lastDate: DateTime.now().add(const Duration(days: 730)),
              initialDate: _start ?? DateTime.now(),
            );
            if (d != null) setState(() => _start = d);
          },
        ),
        ListTile(
          contentPadding: EdgeInsets.zero,
          title: Text(_end == null
              ? 'End date (optional)'
              : 'End: ${_end!.toLocal().toString().split(' ').first}'),
          trailing: const Icon(Icons.calendar_today),
          onTap: () async {
            final d = await showDatePicker(
              context: context,
              firstDate: DateTime.now(),
              lastDate: DateTime.now().add(const Duration(days: 730)),
              initialDate: _end ?? _start ?? DateTime.now(),
            );
            if (d != null) setState(() => _end = d);
          },
        ),
        const SizedBox(height: 12),
        TextField(
          controller: _notes,
          decoration: const InputDecoration(
            labelText: 'Anything else? (dietary, mobility, …)',
          ),
          maxLines: 4,
        ),
      ],
    );
  }
}
