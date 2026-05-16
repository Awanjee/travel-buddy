import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../config/api_config.dart';
import '../models/models.dart';

class ApiClient {
  ApiClient() {
    _dio = Dio(BaseOptions(
      baseUrl: ApiConfig.baseUrl,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 30),
      headers: {'Content-Type': 'application/json'},
    ));
    _dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await _token;
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
    ));
  }

  late final Dio _dio;
  static const _tokenKey = 'auth_token';

  Future<String?> get _token async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_tokenKey);
  }

  Future<void> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_tokenKey, token);
  }

  Future<void> clearToken() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
  }

  Future<bool> get isLoggedIn async => (await _token) != null;

  Future<AuthResponse> register({
    required String email,
    required String password,
    required String displayName,
    String? homeCountryCode,
  }) async {
    final res = await _dio.post('/api/auth/register', data: {
      'email': email,
      'password': password,
      'displayName': displayName,
      'homeCountryCode': homeCountryCode,
    });
    final auth = AuthResponse.fromJson(res.data as Map<String, dynamic>);
    await saveToken(auth.token);
    return auth;
  }

  Future<AuthResponse> login({
    required String email,
    required String password,
  }) async {
    final res = await _dio.post('/api/auth/login', data: {
      'email': email,
      'password': password,
    });
    final auth = AuthResponse.fromJson(res.data as Map<String, dynamic>);
    await saveToken(auth.token);
    return auth;
  }

  Future<void> logout() => clearToken();

  Future<Profile> getProfile() async {
    final res = await _dio.get('/api/profile');
    return Profile.fromJson(res.data as Map<String, dynamic>);
  }

  Future<Profile> updateProfile({
    required String displayName,
    String? homeCountryCode,
    String? passportCountryCode,
    String? bio,
  }) async {
    final res = await _dio.put('/api/profile', data: {
      'displayName': displayName,
      'homeCountryCode': homeCountryCode,
      'passportCountryCode': passportCountryCode,
      'bio': bio,
    });
    return Profile.fromJson(res.data as Map<String, dynamic>);
  }

  Future<List<Trip>> listTrips() async {
    final res = await _dio.get('/api/trips');
    return (res.data as List<dynamic>)
        .map((e) => Trip.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<Trip> createTrip(Map<String, dynamic> body) async {
    final res = await _dio.post('/api/trips', data: body);
    return Trip.fromJson(res.data as Map<String, dynamic>);
  }

  Future<Trip> getTrip(String id) async {
    final res = await _dio.get('/api/trips/$id');
    return Trip.fromJson(res.data as Map<String, dynamic>);
  }

  Future<VisaGuidance> getVisa(String tripId) async {
    final res = await _dio.get('/api/trips/$tripId/visa');
    return VisaGuidance.fromJson(res.data as Map<String, dynamic>);
  }

  Future<List<Candidate>> getPendingCandidates(String tripId) async {
    final res = await _dio.get('/api/trips/$tripId/candidates/pending');
    return (res.data as List<dynamic>)
        .map((e) => Candidate.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<Candidate> decide(String tripId, String candidateId, String decision) async {
    final res = await _dio.post(
      '/api/trips/$tripId/candidates/$candidateId/decision',
      data: {'decision': decision},
    );
    return Candidate.fromJson(res.data as Map<String, dynamic>);
  }

  Future<Itinerary> buildItinerary(String tripId) async {
    final res = await _dio.post('/api/trips/$tripId/itinerary/build');
    return Itinerary.fromJson(res.data as Map<String, dynamic>);
  }

  Future<Itinerary> getItinerary(String tripId) async {
    final res = await _dio.get('/api/trips/$tripId/itinerary');
    return Itinerary.fromJson(res.data as Map<String, dynamic>);
  }

  String pdfUrl(String relativePath) => '${ApiConfig.baseUrl}$relativePath';
}
