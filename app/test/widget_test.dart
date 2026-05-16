import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:app/main.dart';

void main() {
  testWidgets('App loads login when not authenticated', (tester) async {
    await tester.pumpWidget(const ProviderScope(child: TravelBuddyApp()));
    await tester.pumpAndSettle();
    expect(find.text('Travel Buddy'), findsWidgets);
  });
}
