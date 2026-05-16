import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../providers/providers.dart';
import '../screens/auth/login_screen.dart';
import '../screens/auth/register_screen.dart';
import '../screens/home/home_screen.dart';
import '../screens/trip/discovery_screen.dart';
import '../screens/trip/itinerary_screen.dart';
import '../screens/trip/questionnaire_screen.dart';
import '../screens/trip/visa_screen.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final auth = ref.watch(authStateProvider);

  return GoRouter(
    initialLocation: '/',
    refreshListenable: _AuthRefresh(ref),
    redirect: (context, state) {
      final loggedIn = auth.value ?? false;
      final onAuth =
          state.matchedLocation == '/login' ||
          state.matchedLocation == '/register';

      if (!loggedIn && !onAuth) return '/login';
      if (loggedIn && onAuth) return '/';
      return null;
    },
    routes: [
      GoRoute(path: '/login', builder: (_, _) => const LoginScreen()),
      GoRoute(path: '/register', builder: (_, _) => const RegisterScreen()),
      GoRoute(path: '/', builder: (_, _) => const HomeScreen()),
      GoRoute(
        path: '/trips/new',
        builder: (_, _) => const QuestionnaireScreen(),
      ),
      GoRoute(
        path: '/trips/:id/visa',
        builder: (_, state) => VisaScreen(tripId: state.pathParameters['id']!),
      ),
      GoRoute(
        path: '/trips/:id/discover',
        builder: (_, state) =>
            DiscoveryScreen(tripId: state.pathParameters['id']!),
      ),
      GoRoute(
        path: '/trips/:id/plan',
        builder: (_, state) =>
            ItineraryScreen(tripId: state.pathParameters['id']!),
      ),
    ],
  );
});

class _AuthRefresh extends ChangeNotifier {
  _AuthRefresh(this._ref) {
    _ref.listen(authStateProvider, (_, _) => notifyListeners());
  }
  final Ref _ref;
}
