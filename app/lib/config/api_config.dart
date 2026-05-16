import 'package:flutter/foundation.dart';

class ApiConfig {
  static String get baseUrl {
    if (kIsWeb) return 'http://localhost:5280';
    return defaultTargetPlatform == TargetPlatform.android
        ? 'http://10.0.2.2:5280'
        : 'http://localhost:5280';
  }
}
