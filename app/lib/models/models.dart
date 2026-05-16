class AuthResponse {
  AuthResponse({
    required this.token,
    required this.userId,
    required this.email,
    required this.displayName,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) => AuthResponse(
        token: json['token'] as String,
        userId: json['userId'] as String,
        email: json['email'] as String,
        displayName: json['displayName'] as String,
      );

  final String token;
  final String userId;
  final String email;
  final String displayName;
}

class Profile {
  Profile({
    required this.userId,
    required this.email,
    required this.displayName,
    this.homeCountryCode,
    this.passportCountryCode,
    this.bio,
  });

  factory Profile.fromJson(Map<String, dynamic> json) => Profile(
        userId: json['userId'] as String,
        email: json['email'] as String,
        displayName: json['displayName'] as String,
        homeCountryCode: json['homeCountryCode'] as String?,
        passportCountryCode: json['passportCountryCode'] as String?,
        bio: json['bio'] as String?,
      );

  final String userId;
  final String email;
  final String displayName;
  final String? homeCountryCode;
  final String? passportCountryCode;
  final String? bio;
}

class Trip {
  Trip({
    required this.id,
    required this.destinationCountryCode,
    required this.destinationCountryName,
    this.startDate,
    this.endDate,
    required this.partySize,
    required this.budgetBand,
    required this.energyLevel,
    required this.visaIntent,
    required this.preferences,
    this.personalNotes,
    required this.status,
    required this.createdAt,
  });

  factory Trip.fromJson(Map<String, dynamic> json) => Trip(
        id: json['id'] as String,
        destinationCountryCode: json['destinationCountryCode'] as String,
        destinationCountryName: json['destinationCountryName'] as String,
        startDate: json['startDate'] as String?,
        endDate: json['endDate'] as String?,
        partySize: json['partySize'] as int,
        budgetBand: json['budgetBand'] as String,
        energyLevel: json['energyLevel'] as String,
        visaIntent: json['visaIntent'] as String,
        preferences: (json['preferences'] as List<dynamic>).cast<String>(),
        personalNotes: json['personalNotes'] as String?,
        status: json['status'] as String,
        createdAt: DateTime.parse(json['createdAt'] as String),
      );

  final String id;
  final String destinationCountryCode;
  final String destinationCountryName;
  final String? startDate;
  final String? endDate;
  final int partySize;
  final String budgetBand;
  final String energyLevel;
  final String visaIntent;
  final List<String> preferences;
  final String? personalNotes;
  final String status;
  final DateTime createdAt;
}

class VisaGuidance {
  VisaGuidance({
    required this.id,
    required this.summary,
    required this.disclaimer,
    required this.timelineMinDays,
    required this.timelineMaxDays,
    required this.timelineNotes,
    required this.sourceUrl,
    required this.lastVerifiedAt,
    required this.checklist,
  });

  factory VisaGuidance.fromJson(Map<String, dynamic> json) => VisaGuidance(
        id: json['id'] as String,
        summary: json['summary'] as String,
        disclaimer: json['disclaimer'] as String,
        timelineMinDays: json['timelineMinDays'] as int,
        timelineMaxDays: json['timelineMaxDays'] as int,
        timelineNotes: json['timelineNotes'] as String,
        sourceUrl: json['sourceUrl'] as String,
        lastVerifiedAt: DateTime.parse(json['lastVerifiedAt'] as String),
        checklist: (json['checklist'] as List<dynamic>)
            .map((e) => VisaChecklistItem.fromJson(e as Map<String, dynamic>))
            .toList(),
      );

  final String id;
  final String summary;
  final String disclaimer;
  final int timelineMinDays;
  final int timelineMaxDays;
  final String timelineNotes;
  final String sourceUrl;
  final DateTime lastVerifiedAt;
  final List<VisaChecklistItem> checklist;
}

class VisaChecklistItem {
  VisaChecklistItem({
    required this.sortOrder,
    required this.title,
    required this.description,
    required this.isRequired,
  });

  factory VisaChecklistItem.fromJson(Map<String, dynamic> json) =>
      VisaChecklistItem(
        sortOrder: json['sortOrder'] as int,
        title: json['title'] as String,
        description: json['description'] as String,
        isRequired: json['isRequired'] as bool,
      );

  final int sortOrder;
  final String title;
  final String description;
  final bool isRequired;
}

class Candidate {
  Candidate({
    required this.id,
    required this.type,
    required this.tag,
    required this.name,
    required this.description,
    this.location,
    this.imageUrl,
    this.priceEstimateUsd,
    required this.score,
    this.bookingUrl,
    this.decision,
  });

  factory Candidate.fromJson(Map<String, dynamic> json) => Candidate(
        id: json['id'] as String,
        type: json['type'] as String,
        tag: json['tag'] as String,
        name: json['name'] as String,
        description: json['description'] as String,
        location: json['location'] as String?,
        imageUrl: json['imageUrl'] as String?,
        priceEstimateUsd: (json['priceEstimateUsd'] as num?)?.toDouble(),
        score: json['score'] as int,
        bookingUrl: json['bookingUrl'] as String?,
        decision: json['decision'] as String?,
      );

  final String id;
  final String type;
  final String tag;
  final String name;
  final String description;
  final String? location;
  final String? imageUrl;
  final double? priceEstimateUsd;
  final int score;
  final String? bookingUrl;
  final String? decision;
}

class Itinerary {
  Itinerary({
    required this.id,
    required this.versionNumber,
    required this.planMarkdown,
    this.exportPdfUrl,
    required this.createdAt,
  });

  factory Itinerary.fromJson(Map<String, dynamic> json) => Itinerary(
        id: json['id'] as String,
        versionNumber: json['versionNumber'] as int,
        planMarkdown: json['planMarkdown'] as String,
        exportPdfUrl: json['exportPdfUrl'] as String?,
        createdAt: DateTime.parse(json['createdAt'] as String),
      );

  final String id;
  final int versionNumber;
  final String planMarkdown;
  final String? exportPdfUrl;
  final DateTime createdAt;
}
