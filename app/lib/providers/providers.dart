import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../services/api_client.dart';

final apiClientProvider = Provider<ApiClient>((ref) => ApiClient());

final authStateProvider = FutureProvider<bool>((ref) async {
  return ref.watch(apiClientProvider).isLoggedIn;
});
