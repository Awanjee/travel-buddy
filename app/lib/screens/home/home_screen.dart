import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../models/models.dart';
import '../../providers/providers.dart';

class HomeScreen extends ConsumerStatefulWidget {
  const HomeScreen({super.key});

  @override
  ConsumerState<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends ConsumerState<HomeScreen> {
  List<Trip>? _trips;
  var _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final trips = await ref.read(apiClientProvider).listTrips();
      if (mounted) setState(() => _trips = trips);
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _logout() async {
    await ref.read(apiClientProvider).logout();
    ref.invalidate(authStateProvider);
    if (mounted) context.go('/login');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Travel Buddy'),
        actions: [
          IconButton(
            onPressed: _logout,
            icon: const Icon(Icons.logout),
            tooltip: 'Sign out',
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => context.push('/trips/new').then((_) => _load()),
        icon: const Icon(Icons.add),
        label: const Text('New trip'),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _trips == null || _trips!.isEmpty
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(32),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.map_outlined,
                            size: 64,
                            color: Theme.of(context).colorScheme.primary),
                        const SizedBox(height: 16),
                        const Text(
                          'No trips yet',
                          style: TextStyle(
                              fontSize: 20, fontWeight: FontWeight.w600),
                        ),
                        const SizedBox(height: 8),
                        const Text(
                          'Answer a few questions and we\'ll help with visa guidance, places, and your itinerary.',
                          textAlign: TextAlign.center,
                        ),
                      ],
                    ),
                  ),
                )
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.separated(
                    padding: const EdgeInsets.all(16),
                    itemCount: _trips!.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 12),
                    itemBuilder: (context, i) {
                      final trip = _trips![i];
                      return Card(
                        child: ListTile(
                          title: Text(trip.destinationCountryName),
                          subtitle: Text(
                            '${trip.status} · ${trip.partySize} traveler(s)',
                          ),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => _openTrip(trip),
                        ),
                      );
                    },
                  ),
                ),
    );
  }

  void _openTrip(Trip trip) {
    showModalBottomSheet(
      context: context,
      showDragHandle: true,
      builder: (ctx) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.article_outlined),
              title: const Text('Visa guidance'),
              onTap: () {
                Navigator.pop(ctx);
                context.push('/trips/${trip.id}/visa');
              },
            ),
            ListTile(
              leading: const Icon(Icons.explore_outlined),
              title: const Text('Discover & approve'),
              onTap: () {
                Navigator.pop(ctx);
                context.push('/trips/${trip.id}/discover');
              },
            ),
            ListTile(
              leading: const Icon(Icons.event_note),
              title: const Text('Your plan'),
              onTap: () {
                Navigator.pop(ctx);
                context.push('/trips/${trip.id}/plan');
              },
            ),
          ],
        ),
      ),
    );
  }
}
