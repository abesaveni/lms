using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.Infrastructure.Data;

public static class DbInitializer
{
    // HARDCODED SUPER ADMIN - CANNOT BE CHANGED OR DELETED
    private const string SUPER_ADMIN_EMAIL = "superadmin@liveexpert.ai";
    private const string SUPER_ADMIN_USERNAME = "superadmin";
    private const string SUPER_ADMIN_PASSWORD = "SuperAdmin@2026!";
    private const string SUPER_ADMIN_FIRST_NAME = "Super";
    private const string SUPER_ADMIN_LAST_NAME = "Admin";

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // REPAIR DATABASE (SQLite fix for missing columns)
        try
        {
            var dbType = context.Database.ProviderName;
            if (dbType != null && dbType.Contains("Sqlite"))
            {
                using var connection = context.Database.GetDbConnection();
                connection.Open();
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "PRAGMA table_info(Sessions)";
                bool columnExists = false;
                using var reader = checkCmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["name"].ToString() == "IsReminderSent")
                    {
                        columnExists = true;
                        break;
                    }
                }
                reader.Close();

                if (!columnExists)
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE Sessions ADD COLUMN IsReminderSent INTEGER DEFAULT 0 NOT NULL";
                    alterCmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("✓ Repaired database: Added IsReminderSent column to Sessions table.");
                }

                // Check for DateOfBirth, Location, and Bio in Users table
                checkCmd.CommandText = "PRAGMA table_info(Users)";
                bool dobExists = false;
                bool locationExists = false;
                bool bioExists = false;
                using var userReader = checkCmd.ExecuteReader();
                while (userReader.Read())
                {
                    var colName = userReader["name"].ToString();
                    if (colName == "DateOfBirth") dobExists = true;
                    if (colName == "Location") locationExists = true;
                    if (colName == "Bio") bioExists = true;
                }
                userReader.Close();

                if (!bioExists)
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE Users ADD COLUMN Bio TEXT NULL";
                    alterCmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("✓ Repaired database: Added Bio column to Users table.");
                }
                if (!dobExists)
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE Users ADD COLUMN DateOfBirth TEXT NULL";
                    alterCmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("✓ Repaired database: Added DateOfBirth column to Users table.");
                }
                if (!locationExists)
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE Users ADD COLUMN Location TEXT NULL";
                    alterCmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("✓ Repaired database: Added Location column to Users table.");
                }

                // Check for HourlyRateGroup in TutorProfiles table
                checkCmd.CommandText = "PRAGMA table_info(TutorProfiles)";
                bool hourlyRateGroupExists = false;
                using var tutorReader = checkCmd.ExecuteReader();
                while (tutorReader.Read())
                {
                    if (tutorReader["name"].ToString() == "HourlyRateGroup") { hourlyRateGroupExists = true; break; }
                }
                tutorReader.Close();
                if (!hourlyRateGroupExists)
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN HourlyRateGroup REAL NOT NULL DEFAULT 0";
                    alterCmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("✓ Repaired database: Added HourlyRateGroup column to TutorProfiles table.");
                }

                // Check if TutorSubjectRates table exists
                checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='TutorSubjectRates'";
                var subjectRatesTable = checkCmd.ExecuteScalar();
                if (subjectRatesTable == null)
                {
                    using var createSubjectRatesCmd = connection.CreateCommand();
                    createSubjectRatesCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS TutorSubjectRates (
                            Id TEXT NOT NULL PRIMARY KEY,
                            TutorId TEXT NOT NULL,
                            SubjectId TEXT NULL,
                            SubjectName TEXT NOT NULL DEFAULT '',
                            HourlyRate REAL NOT NULL DEFAULT 0,
                            TrialRate REAL NULL,
                            IsActive INTEGER NOT NULL DEFAULT 1,
                            DisplayOrder INTEGER NOT NULL DEFAULT 0,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL,
                            FOREIGN KEY (TutorId) REFERENCES Users(Id) ON DELETE CASCADE
                        )";
                    createSubjectRatesCmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("✓ Repaired database: Created TutorSubjectRates table.");
                }

                // Check if UserCalendarConnections table exists
                checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='UserCalendarConnections'";
                var calConnTable = checkCmd.ExecuteScalar();
                if (calConnTable == null)
                {
                    using var createCalConnCmd = connection.CreateCommand();
                    createCalConnCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS UserCalendarConnections (
                            Id TEXT NOT NULL PRIMARY KEY,
                            UserId TEXT NOT NULL,
                            Provider INTEGER NOT NULL DEFAULT 0,
                            AccessToken TEXT NOT NULL,
                            RefreshToken TEXT NOT NULL,
                            TokenExpiry TEXT NOT NULL,
                            GoogleEmail TEXT NOT NULL DEFAULT '',
                            IsActive INTEGER NOT NULL DEFAULT 1,
                            LastRefreshedAt TEXT NULL,
                            ConnectedAt TEXT NOT NULL,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        )";
                    createCalConnCmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("✓ Repaired database: Created UserCalendarConnections table.");
                }

                // Check if SessionMeetLinks table exists (added after initial DB creation)
                checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='SessionMeetLinks'";
                var tableResult = checkCmd.ExecuteScalar();
                if (tableResult == null)
                {
                    using var createTableCmd = connection.CreateCommand();
                    createTableCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS SessionMeetLinks (
                            Id TEXT NOT NULL PRIMARY KEY,
                            SessionId TEXT NOT NULL UNIQUE,
                            MeetUrl TEXT NOT NULL,
                            CalendarEventId TEXT NULL,
                            IsActive INTEGER NOT NULL DEFAULT 1,
                            SessionStartedAt TEXT NULL,
                            SessionEndedAt TEXT NULL,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL,
                            FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
                        )";
                    createTableCmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("✓ Repaired database: Created SessionMeetLinks table.");
                }

                // ── Flash Sale + Instant Booking + No-Show + RequiresSubscription on Sessions ──
                checkCmd.CommandText = "PRAGMA table_info(Sessions)";
                bool flashSalePriceExists = false, flashSaleEndsAtExists = false;
                bool instantBookingExists = false, noShowProtectionExists = false, requiresSubscriptionExists = false;
                using (var sessReader = checkCmd.ExecuteReader())
                {
                    while (sessReader.Read())
                    {
                        var col = sessReader["name"].ToString();
                        if (col == "FlashSalePrice") flashSalePriceExists = true;
                        if (col == "FlashSaleEndsAt") flashSaleEndsAtExists = true;
                        if (col == "InstantBooking") instantBookingExists = true;
                        if (col == "NoShowProtection") noShowProtectionExists = true;
                        if (col == "RequiresSubscription") requiresSubscriptionExists = true;
                    }
                }
                if (!flashSalePriceExists) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE Sessions ADD COLUMN FlashSalePrice TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!flashSaleEndsAtExists) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE Sessions ADD COLUMN FlashSaleEndsAt TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!instantBookingExists) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE Sessions ADD COLUMN InstantBooking INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
                if (!noShowProtectionExists) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE Sessions ADD COLUMN NoShowProtection INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
                if (!requiresSubscriptionExists) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE Sessions ADD COLUMN RequiresSubscription INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }

                // ── TutorProfiles: all new columns ───────────────────────────────────
                checkCmd.CommandText = "PRAGMA table_info(TutorProfiles)";
                bool hasBgCheck = false, bgCheckDate = false, tpVerifReminder = false;
                bool tpTeachingStyles = false, tpAgeGroups = false;
                bool tpTrialAvailable = false, tpTrialDuration = false, tpTrialPrice = false;
                using (var tpReader = checkCmd.ExecuteReader())
                {
                    while (tpReader.Read())
                    {
                        var col = tpReader["name"].ToString();
                        if (col == "HasBackgroundCheck") hasBgCheck = true;
                        if (col == "BackgroundCheckDate") bgCheckDate = true;
                        if (col == "VerificationReminderSent") tpVerifReminder = true;
                        if (col == "TeachingStyles") tpTeachingStyles = true;
                        if (col == "AgeGroups") tpAgeGroups = true;
                        if (col == "TrialAvailable") tpTrialAvailable = true;
                        if (col == "TrialDurationMinutes") tpTrialDuration = true;
                        if (col == "TrialPrice") tpTrialPrice = true;
                    }
                }
                if (!hasBgCheck) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN HasBackgroundCheck INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
                if (!bgCheckDate) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN BackgroundCheckDate TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!tpVerifReminder) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN VerificationReminderSent INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
                if (!tpTeachingStyles) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN TeachingStyles TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!tpAgeGroups) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN AgeGroups TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!tpTrialAvailable) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN TrialAvailable INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
                if (!tpTrialDuration) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN TrialDurationMinutes INTEGER NOT NULL DEFAULT 30"; c2.ExecuteNonQuery(); }
                if (!tpTrialPrice) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN TrialPrice TEXT NOT NULL DEFAULT '0'"; c2.ExecuteNonQuery(); }

                // ── CouponDiscount on SessionBookings ─────────────────────────────────
                checkCmd.CommandText = "PRAGMA table_info(SessionBookings)";
                bool couponDiscountExists = false;
                using (var sbReader = checkCmd.ExecuteReader())
                {
                    while (sbReader.Read())
                    {
                        if (sbReader["name"].ToString() == "CouponDiscount") { couponDiscountExists = true; break; }
                    }
                }
                if (!couponDiscountExists) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE SessionBookings ADD COLUMN CouponDiscount TEXT NOT NULL DEFAULT '0'"; c2.ExecuteNonQuery(); }

                // ── Create CouponCodes table if missing ───────────────────────────────
                checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='CouponCodes'";
                if (checkCmd.ExecuteScalar() == null)
                {
                    using var c2 = connection.CreateCommand();
                    c2.CommandText = @"
                        CREATE TABLE IF NOT EXISTS CouponCodes (
                            Id TEXT NOT NULL PRIMARY KEY,
                            Code TEXT NOT NULL,
                            Description TEXT NULL,
                            DiscountType INTEGER NOT NULL DEFAULT 0,
                            DiscountValue TEXT NOT NULL DEFAULT '0',
                            MaxDiscountAmount TEXT NULL,
                            MinOrderAmount TEXT NULL,
                            MaxUses INTEGER NULL,
                            UsedCount INTEGER NOT NULL DEFAULT 0,
                            ExpiresAt TEXT NULL,
                            IsActive INTEGER NOT NULL DEFAULT 1,
                            TutorId TEXT NULL,
                            CreatedByAdminId TEXT NULL,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL
                        )";
                    c2.ExecuteNonQuery();
                }

                // ── Create CouponUsages table if missing ──────────────────────────────
                checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='CouponUsages'";
                if (checkCmd.ExecuteScalar() == null)
                {
                    using var c2 = connection.CreateCommand();
                    c2.CommandText = @"
                        CREATE TABLE IF NOT EXISTS CouponUsages (
                            Id TEXT NOT NULL PRIMARY KEY,
                            CouponId TEXT NOT NULL,
                            StudentId TEXT NOT NULL,
                            BookingId TEXT NOT NULL,
                            DiscountApplied TEXT NOT NULL DEFAULT '0',
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL,
                            FOREIGN KEY (CouponId) REFERENCES CouponCodes(Id) ON DELETE CASCADE
                        )";
                    c2.ExecuteNonQuery();
                }

                // ── StudentProfiles: ReferralCode, ReferredBy, ResumeData cols ──────
                checkCmd.CommandText = "PRAGMA table_info(StudentProfiles)";
                bool spReferralCode = false, spReferredBy = false, spResumeData = false, spResumeType = false, spResumeUpdated = false;
                using (var spReader = checkCmd.ExecuteReader())
                {
                    while (spReader.Read())
                    {
                        var col = spReader["name"].ToString();
                        if (col == "ReferralCode") spReferralCode = true;
                        if (col == "ReferredBy") spReferredBy = true;
                        if (col == "ResumeData") spResumeData = true;
                        if (col == "ResumeType") spResumeType = true;
                        if (col == "ResumeLastUpdatedAt") spResumeUpdated = true;
                    }
                }
                if (!spReferralCode) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentProfiles ADD COLUMN ReferralCode TEXT NOT NULL DEFAULT ''"; c2.ExecuteNonQuery(); }
                if (!spReferredBy) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentProfiles ADD COLUMN ReferredBy TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!spResumeData) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentProfiles ADD COLUMN ResumeData TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!spResumeType) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentProfiles ADD COLUMN ResumeType TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!spResumeUpdated) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentProfiles ADD COLUMN ResumeLastUpdatedAt TEXT NULL"; c2.ExecuteNonQuery(); }

                // ── ReferralPrograms: ExpiresAt + IsTutorReferral ─────────────────
                checkCmd.CommandText = "PRAGMA table_info(ReferralPrograms)";
                bool rpExpires = false, rpIsTutor = false;
                using (var rpReader = checkCmd.ExecuteReader())
                {
                    while (rpReader.Read())
                    {
                        var col = rpReader["name"].ToString();
                        if (col == "ExpiresAt") rpExpires = true;
                        if (col == "IsTutorReferral") rpIsTutor = true;
                    }
                }
                if (!rpExpires) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE ReferralPrograms ADD COLUMN ExpiresAt TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!rpIsTutor) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE ReferralPrograms ADD COLUMN IsTutorReferral INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }

                // ── TutorProfiles: TutorReferralCode, AutoPayoutSchedule, AutoPayoutMinimumAmount ──
                // (re-use the same tpReader pass — check vars set above)
                bool tpTutorRefCode = false, tpAutoPayout = false, tpAutoPayoutMin = false;
                checkCmd.CommandText = "PRAGMA table_info(TutorProfiles)";
                using (var tpReader2 = checkCmd.ExecuteReader())
                {
                    while (tpReader2.Read())
                    {
                        var col = tpReader2["name"].ToString();
                        if (col == "TutorReferralCode") tpTutorRefCode = true;
                        if (col == "AutoPayoutSchedule") tpAutoPayout = true;
                        if (col == "AutoPayoutMinimumAmount") tpAutoPayoutMin = true;
                    }
                }
                if (!tpTutorRefCode) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN TutorReferralCode TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!tpAutoPayout) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN AutoPayoutSchedule INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
                if (!tpAutoPayoutMin) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE TutorProfiles ADD COLUMN AutoPayoutMinimumAmount TEXT NOT NULL DEFAULT '1000'"; c2.ExecuteNonQuery(); }

                // ── StudentSubscriptions: new feature cols ────────────────────────
                checkCmd.CommandText = "PRAGMA table_info(StudentSubscriptions)";
                bool ssSessionsUsed = false, ssAutoRenew = false, ssRenewal = false, ssCancelReason = false;
                bool ssRetDisc = false, ssRetPct = false, ssRetExp = false, ssPending = false;
                using (var ssReader = checkCmd.ExecuteReader())
                {
                    while (ssReader.Read())
                    {
                        var col = ssReader["name"].ToString();
                        if (col == "SessionsUsed") ssSessionsUsed = true;
                        if (col == "AutoRenew") ssAutoRenew = true;
                        if (col == "RenewalReminderSentAt") ssRenewal = true;
                        if (col == "CancellationReason") ssCancelReason = true;
                        if (col == "RetentionDiscountOffered") ssRetDisc = true;
                        if (col == "RetentionDiscountPercent") ssRetPct = true;
                        if (col == "RetentionOfferExpiry") ssRetExp = true;
                        if (col == "PendingCancellation") ssPending = true;
                    }
                }
                if (!ssSessionsUsed) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentSubscriptions ADD COLUMN SessionsUsed INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
                if (!ssAutoRenew) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentSubscriptions ADD COLUMN AutoRenew INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
                if (!ssRenewal) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentSubscriptions ADD COLUMN RenewalReminderSentAt TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!ssCancelReason) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentSubscriptions ADD COLUMN CancellationReason TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!ssRetDisc) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentSubscriptions ADD COLUMN RetentionDiscountOffered INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
                if (!ssRetPct) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentSubscriptions ADD COLUMN RetentionDiscountPercent TEXT NOT NULL DEFAULT '0'"; c2.ExecuteNonQuery(); }
                if (!ssRetExp) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentSubscriptions ADD COLUMN RetentionOfferExpiry TEXT NULL"; c2.ExecuteNonQuery(); }
                if (!ssPending) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE StudentSubscriptions ADD COLUMN PendingCancellation INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }

                // ── SubscriptionPlans: SessionsLimit ──────────────────────────────
                checkCmd.CommandText = "PRAGMA table_info(SubscriptionPlans)";
                bool spSessionsLimit = false;
                using (var splReader = checkCmd.ExecuteReader())
                {
                    while (splReader.Read())
                    {
                        if (splReader["name"].ToString() == "SessionsLimit") { spSessionsLimit = true; break; }
                    }
                }
                if (!spSessionsLimit) { using var c2 = connection.CreateCommand(); c2.CommandText = "ALTER TABLE SubscriptionPlans ADD COLUMN SessionsLimit INTEGER NOT NULL DEFAULT 0"; c2.ExecuteNonQuery(); }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠ Database repair failed: {ex.Message}");
        }

        // Ensure database is ready
        await context.Database.EnsureCreatedAsync();

        // CREATE HARDCODED SUPER ADMIN (CANNOT BE CHANGED)
        var superAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == SUPER_ADMIN_EMAIL);
        if (superAdmin == null)
        {
            superAdmin = new User
            {
                Id = Guid.NewGuid(),
                Username = SUPER_ADMIN_USERNAME,
                Email = SUPER_ADMIN_EMAIL,
                FirstName = SUPER_ADMIN_FIRST_NAME,
                LastName = SUPER_ADMIN_LAST_NAME,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(SUPER_ADMIN_PASSWORD),
                Role = UserRole.Admin,
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(superAdmin);
            await context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"✓ Super Admin created: {SUPER_ADMIN_EMAIL} / {SUPER_ADMIN_PASSWORD}");
        }
        else
        {
            // Always ensure super admin password is correct (cannot be changed by anyone)
            superAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(SUPER_ADMIN_PASSWORD);
            superAdmin.IsActive = true;
            superAdmin.IsEmailVerified = true;
            superAdmin.Role = UserRole.Admin; // Ensure role is always Admin
            superAdmin.UpdatedAt = DateTime.UtcNow;
            context.Users.Update(superAdmin);
            await context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"✓ Super Admin credentials reset: {SUPER_ADMIN_EMAIL} / {SUPER_ADMIN_PASSWORD}");
        }

        // Seed / self-heal subjects — adds any missing subjects even on existing databases
        var allSubjectDefs = new List<(string Name, string Slug, string Description)>
        {
            // STEM
            ("Mathematics", "mathematics", "Calculus, Algebra, Geometry, Statistics"),
            ("Physics", "physics", "Classical Mechanics, Quantum Physics, Thermodynamics"),
            ("Chemistry", "chemistry", "Organic, Inorganic, Physical Chemistry"),
            ("Biology", "biology", "Cell Biology, Genetics, Ecology, Human Anatomy"),
            ("Computer Science", "computer-science", "Algorithms, Data Structures, OOP, OS"),
            ("Statistics", "statistics", "Probability, Hypothesis Testing, Data Analysis"),
            // Languages & Literature
            ("English Literature", "english-literature", "Modern & Classical Literature, Writing"),
            ("English Language", "english-language", "Grammar, Writing, Reading Comprehension"),
            ("Hindi", "hindi", "Hindi Language, Literature, Grammar"),
            ("French", "french", "French Language, Grammar, Conversation"),
            ("Spanish", "spanish", "Spanish Language, Grammar, Conversation"),
            // Technology
            ("Web Development", "web-development", "React, Node.js, HTML/CSS, JavaScript"),
            ("Data Science", "data-science", "Python, Machine Learning, AI, Statistics"),
            ("Mobile Development", "mobile-development", "iOS, Android, React Native, Flutter"),
            ("Cybersecurity", "cybersecurity", "Network Security, Ethical Hacking, Cryptography"),
            ("Cloud Computing", "cloud-computing", "AWS, Azure, GCP, DevOps"),
            ("Database Management", "database-management", "SQL, NoSQL, MongoDB, PostgreSQL"),
            // Commerce & Humanities
            ("Business Studies", "business-studies", "Marketing, Finance, Management, Strategy"),
            ("Economics", "economics", "Microeconomics, Macroeconomics, Business Economics"),
            ("Accountancy", "accountancy", "Financial Accounting, Cost Accounting, Taxation"),
            ("History", "history", "World History, Indian History, Political History"),
            ("Geography", "geography", "Physical Geography, Human Geography, Map Work"),
            ("Political Science", "political-science", "Political Theory, Governance, International Relations"),
            ("Psychology", "psychology", "Cognitive, Developmental, Clinical Psychology"),
            ("Sociology", "sociology", "Social Theory, Research Methods, Culture"),
            // Test Prep & Other
            ("JEE Preparation", "jee-preparation", "Physics, Chemistry, Maths for IIT-JEE"),
            ("NEET Preparation", "neet-preparation", "Biology, Physics, Chemistry for NEET"),
            ("CAT / MBA Prep", "cat-mba-prep", "Quantitative Aptitude, Verbal Ability, LRDI"),
            ("UPSC / Civil Services", "upsc-civil-services", "GS, Optional Subjects, Essay, CSAT"),
            ("Music", "music", "Classical, Western, Vocals, Instruments"),
            ("Art & Drawing", "art-drawing", "Sketching, Painting, Digital Art"),
        };

        var existingSlugs = context.Subjects.Select(s => s.Slug).ToHashSet();
        var newSubjects = allSubjectDefs
            .Where(d => !existingSlugs.Contains(d.Slug))
            .Select(d => new Subject
            {
                Id = Guid.NewGuid(),
                Name = d.Name,
                Slug = d.Slug,
                Description = d.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (newSubjects.Any())
        {
            context.Subjects.AddRange(newSubjects);
            await context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"✓ Seeded {newSubjects.Count} subjects (total now: {existingSlugs.Count + newSubjects.Count}).");
        }

        await context.SaveChangesAsync();

        // Seed default system settings
        if (!context.SystemSettings.Any())
        {
            var defaultSettings = new List<SystemSetting>
            {
                new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    SettingKey = "PlatformFeeEnabled",
                    SettingValue = "true",
                    DataType = "Bool",
                    Description = "Enable/disable platform fee for session bookings",
                    IsEncrypted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    SettingKey = "PlatformFeeType",
                    SettingValue = "Fixed",
                    DataType = "String",
                    Description = "Platform fee type: Fixed, PerHour, Percentage",
                    IsEncrypted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    SettingKey = "PlatformFeeFixed",
                    SettingValue = "100.00",
                    DataType = "Decimal",
                    Description = "Fixed platform fee per session",
                    IsEncrypted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    SettingKey = "PlatformFeePerHour",
                    SettingValue = "50.00",
                    DataType = "Decimal",
                    Description = "Platform fee per hour for hourly sessions",
                    IsEncrypted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    SettingKey = "PlatformFeePercentage",
                    SettingValue = "0.00",
                    DataType = "Decimal",
                    Description = "Platform fee percentage of base amount",
                    IsEncrypted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    SettingKey = "MinWithdrawalAmount",
                    SettingValue = "1000.00",
                    DataType = "Decimal",
                    Description = "Minimum withdrawal amount in INR",
                    IsEncrypted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    SettingKey = "ReferralBonusCredits",
                    SettingValue = "50.00",
                    DataType = "Decimal",
                    Description = "Bonus points awarded for successful referrals",
                    IsEncrypted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    SettingKey = "RegistrationBonusCredits",
                    SettingValue = "100.00",
                    DataType = "Decimal",
                    Description = "Bonus points given to new users on registration",
                    IsEncrypted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.SystemSettings.AddRange(defaultSettings);
        }

        // Seed daily challenges (rolling 45-day window: 7 past + today + 37 future)
        await SeedDailyChallengesAsync(context);

        try
        {
            await context.SaveChangesAsync();

            // Log seeding completion
            var userCount = await context.Users.CountAsync();
            System.Diagnostics.Debug.WriteLine($"Database seeding completed. Total users: {userCount}");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqlEx && sqlEx.Message.Contains("UNIQUE constraint"))
        {
            // Handle unique constraint violations - likely referral codes
            System.Diagnostics.Debug.WriteLine($"Unique constraint violation during seeding: {sqlEx.Message}");
            // Try to fix by regenerating referral codes for students without them
            var studentsWithoutCodes = await context.StudentProfiles
                .Where(sp => string.IsNullOrEmpty(sp.ReferralCode))
                .ToListAsync();
            
            var random = new Random();
            foreach (var student in studentsWithoutCodes)
            {
                string referralCode;
                bool isUnique = false;
                int attempts = 0;
                
                do
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Id == student.UserId);
                    var username = user?.Username ?? "STU";
                    var prefix = username.ToUpper().Substring(0, Math.Min(3, username.Length));
                    referralCode = $"{prefix}{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
                    var exists = await context.StudentProfiles.AnyAsync(sp => sp.ReferralCode == referralCode && sp.Id != student.Id);
                    isUnique = !exists;
                    attempts++;
                    
                    if (attempts > 100)
                    {
                        referralCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                        break;
                    }
                } while (!isUnique);
                
                student.ReferralCode = referralCode;
            }
            
            await context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine("Fixed referral codes and completed seeding");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Daily Challenges Seeder
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task SeedDailyChallengesAsync(ApplicationDbContext context)
    {
        var today = DateTime.UtcNow.Date;

        // Pool of 6 challenges per type (36 total, cycling every 6 days)
        var pool = BuildChallengePool();

        // Generate dates: 7 days back → 37 days forward (45 days total)
        for (int offset = -7; offset <= 37; offset++)
        {
            var date = today.AddDays(offset);
            if (await context.DailyChallenges.AnyAsync(c => c.ChallengeDate == date))
                continue;

            // Cycle through pool: abs(day-of-year) mod pool-length
            int poolIndex = ((int)Math.Abs((date - new DateTime(2026, 1, 1)).TotalDays)) % pool.Count;
            var template = pool[poolIndex];

            context.DailyChallenges.Add(new DailyChallenge
            {
                Id = Guid.NewGuid(),
                ChallengeDate = date,
                Type = template.Type,
                Title = template.Title,
                Description = template.Description,
                ContentJson = template.ContentJson,
                AnswerJson = template.AnswerJson,
                XpReward = template.XpReward,
                Difficulty = template.Difficulty,
                Tag = template.Tag,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    private record ChallengeTemplate(
        ChallengeType Type, string Title, string Description,
        string ContentJson, string AnswerJson,
        int XpReward, string Difficulty, string Tag);

    private static List<ChallengeTemplate> BuildChallengePool() => new()
    {
        // ── Word Scramble ──────────────────────────────────────────────────
        new(ChallengeType.WordScramble, "Word Scramble: OOP Pillar",
            "Unscramble this fundamental OOP concept.",
            """{"scrambled":"NOITALSUPACNE","hint":"Bundling data and methods that operate on it","category":"OOP"}""",
            """{"word":"ENCAPSULATION"}""", 10, "Medium", "OOP"),

        new(ChallengeType.WordScramble, "Word Scramble: CS Concept",
            "Rearrange the letters to reveal a CS term.",
            """{"scrambled":"NOISRUCER","hint":"A function that calls itself","category":"Algorithms"}""",
            """{"word":"RECURSION"}""", 10, "Easy", "Algorithms"),

        new(ChallengeType.WordScramble, "Word Scramble: React",
            "Unscramble this popular frontend framework.",
            """{"scrambled":"MSEIOPSNTMHLIROY","hint":"Multiple implementations of the same interface","category":"OOP"}""",
            """{"word":"POLYMORPHISM"}""", 15, "Hard", "OOP"),

        new(ChallengeType.WordScramble, "Word Scramble: Web Tech",
            "Unscramble this popular JavaScript superset.",
            """{"scrambled":"PTYIPSCRTE","hint":"Adds static typing to JavaScript","category":"Languages"}""",
            """{"word":"TYPESCRIPT"}""", 10, "Easy", "Languages"),

        new(ChallengeType.WordScramble, "Word Scramble: Database",
            "Rearrange to find a key database concept.",
            """{"scrambled":"TNRNOAZOILNAM","hint":"Organising data to reduce redundancy","category":"Databases"}""",
            """{"word":"NORMALIZATION"}""", 15, "Hard", "Databases"),

        new(ChallengeType.WordScramble, "Word Scramble: Design",
            "Unscramble this software design term.",
            """{"scrambled":"TARSCIBONTA","hint":"Hiding complexity behind a simple interface","category":"OOP"}""",
            """{"word":"ABSTRACTION"}""", 10, "Medium", "OOP"),

        // ── Quiz (MCQ) ─────────────────────────────────────────────────────
        new(ChallengeType.Quiz, "Daily Quiz: REST APIs",
            "Test your knowledge of REST API fundamentals.",
            """{"question":"What does REST stand for?","options":["Really Easy State Transfer","Representational State Transfer","Remote Execution State Technology","Resource Endpoint Service Transfer"],"timeLimit":30,"category":"Web Development"}""",
            """{"correct":1}""", 10, "Easy", "Web Development"),

        new(ChallengeType.Quiz, "Daily Quiz: HTTP Status Codes",
            "Which status code should you return for a successful resource creation?",
            """{"question":"Which HTTP status code indicates that a new resource was successfully created?","options":["200 OK","201 Created","204 No Content","301 Moved Permanently"],"timeLimit":25,"category":"HTTP"}""",
            """{"correct":1}""", 10, "Easy", "HTTP"),

        new(ChallengeType.Quiz, "Daily Quiz: React Hooks",
            "Pick the right hook for the scenario.",
            """{"question":"Which React hook would you use to run code after the component mounts and when a dependency changes?","options":["useState","useRef","useEffect","useMemo"],"timeLimit":30,"category":"React"}""",
            """{"correct":2}""", 10, "Medium", "React"),

        new(ChallengeType.Quiz, "Daily Quiz: Git",
            "Test your Git command knowledge.",
            """{"question":"Which git command creates a new branch AND switches to it in one step?","options":["git branch new-feature","git checkout new-feature","git checkout -b new-feature","git switch new-feature"],"timeLimit":30,"category":"Git"}""",
            """{"correct":2}""", 10, "Easy", "Git"),

        new(ChallengeType.Quiz, "Daily Quiz: Big O",
            "Complexity analysis challenge.",
            """{"question":"What is the time complexity of binary search on a sorted array of n elements?","options":["O(n)","O(n²)","O(log n)","O(1)"],"timeLimit":35,"category":"Algorithms"}""",
            """{"correct":2}""", 15, "Medium", "Algorithms"),

        new(ChallengeType.Quiz, "Daily Quiz: CSS",
            "Pick the correct CSS flexbox property.",
            """{"question":"Which CSS property is used to align flex items along the cross axis?","options":["justify-content","align-items","flex-direction","flex-wrap"],"timeLimit":25,"category":"CSS"}""",
            """{"correct":1}""", 10, "Easy", "CSS"),

        // ── Match Pairs ────────────────────────────────────────────────────
        new(ChallengeType.MatchPairs, "Match: OOP Pillars",
            "Match each OOP pillar to its definition.",
            """{"pairs":[{"id":"A","left":"Encapsulation"},{"id":"B","left":"Polymorphism"},{"id":"C","left":"Inheritance"},{"id":"D","left":"Abstraction"}],"definitions":[{"id":"1","right":"Child class derives behaviour from parent"},{"id":"2","right":"One interface, many implementations"},{"id":"3","right":"Hiding complexity behind simple interface"},{"id":"4","right":"Bundling data with its methods"}]}""",
            """{"matches":{"A":"4","B":"2","C":"1","D":"3"}}""", 15, "Medium", "OOP"),

        new(ChallengeType.MatchPairs, "Match: HTTP Methods",
            "Match each HTTP method to its typical use case.",
            """{"pairs":[{"id":"A","left":"GET"},{"id":"B","left":"POST"},{"id":"C","left":"PUT"},{"id":"D","left":"DELETE"}],"definitions":[{"id":"1","right":"Remove a resource"},{"id":"2","right":"Retrieve a resource"},{"id":"3","right":"Replace an entire resource"},{"id":"4","right":"Create a new resource"}]}""",
            """{"matches":{"A":"2","B":"4","C":"3","D":"1"}}""", 10, "Easy", "HTTP"),

        new(ChallengeType.MatchPairs, "Match: Data Structures",
            "Match data structures to their defining property.",
            """{"pairs":[{"id":"A","left":"Stack"},{"id":"B","left":"Queue"},{"id":"C","left":"HashMap"},{"id":"D","left":"LinkedList"}],"definitions":[{"id":"1","right":"O(1) key-value lookup"},{"id":"2","right":"FIFO ordering"},{"id":"3","right":"Nodes pointing to next element"},{"id":"4","right":"LIFO ordering"}]}""",
            """{"matches":{"A":"4","B":"2","C":"1","D":"3"}}""", 15, "Medium", "Data Structures"),

        new(ChallengeType.MatchPairs, "Match: Design Patterns",
            "Match each design pattern to its intent.",
            """{"pairs":[{"id":"A","left":"Singleton"},{"id":"B","left":"Observer"},{"id":"C","left":"Factory"},{"id":"D","left":"Decorator"}],"definitions":[{"id":"1","right":"Add behaviour without modifying class"},{"id":"2","right":"Only one instance allowed"},{"id":"3","right":"Create objects without specifying exact class"},{"id":"4","right":"Notify subscribers of state changes"}]}""",
            """{"matches":{"A":"2","B":"4","C":"3","D":"1"}}""", 20, "Hard", "Design Patterns"),

        new(ChallengeType.MatchPairs, "Match: React Hooks",
            "Match each React hook to its primary purpose.",
            """{"pairs":[{"id":"A","left":"useState"},{"id":"B","left":"useEffect"},{"id":"C","left":"useContext"},{"id":"D","left":"useRef"}],"definitions":[{"id":"1","right":"Access context value without prop drilling"},{"id":"2","right":"Persist a mutable value without re-render"},{"id":"3","right":"Run side effects after render"},{"id":"4","right":"Manage local component state"}]}""",
            """{"matches":{"A":"4","B":"3","C":"1","D":"2"}}""", 15, "Medium", "React"),

        new(ChallengeType.MatchPairs, "Match: SQL Clauses",
            "Match each SQL clause to what it does.",
            """{"pairs":[{"id":"A","left":"WHERE"},{"id":"B","left":"GROUP BY"},{"id":"C","left":"HAVING"},{"id":"D","left":"ORDER BY"}],"definitions":[{"id":"1","right":"Sort result set"},{"id":"2","right":"Filter rows after aggregation"},{"id":"3","right":"Filter rows before aggregation"},{"id":"4","right":"Aggregate rows into groups"}]}""",
            """{"matches":{"A":"3","B":"4","C":"2","D":"1"}}""", 15, "Medium", "Databases"),

        // ── Fill in the Blank ──────────────────────────────────────────────
        new(ChallengeType.FillBlank, "Fill the Blank: React",
            "Choose the correct word to complete the statement.",
            """{"sentence":"In React, the ___ hook is used to manage side effects like API calls and subscriptions.","options":["useState","useEffect","useContext","useRef"],"category":"React"}""",
            """{"answer":"useEffect"}""", 10, "Easy", "React"),

        new(ChallengeType.FillBlank, "Fill the Blank: JavaScript",
            "Pick the right keyword to complete the JS snippet.",
            """{"sentence":"The ___ keyword in JavaScript creates a block-scoped variable that cannot be reassigned.","options":["var","let","const","static"],"category":"JavaScript"}""",
            """{"answer":"const"}""", 10, "Easy", "JavaScript"),

        new(ChallengeType.FillBlank, "Fill the Blank: Git",
            "Complete the Git workflow statement.",
            """{"sentence":"To save your changes to a local Git repository you use git ___ after staging files with git add.","options":["push","pull","commit","merge"],"category":"Git"}""",
            """{"answer":"commit"}""", 10, "Easy", "Git"),

        new(ChallengeType.FillBlank, "Fill the Blank: CSS",
            "Select the right CSS value to complete the rule.",
            """{"sentence":"To centre a flex container's children horizontally, set ___ to center on the parent.","options":["align-items","flex-direction","justify-content","flex-wrap"],"category":"CSS"}""",
            """{"answer":"justify-content"}""", 10, "Easy", "CSS"),

        new(ChallengeType.FillBlank, "Fill the Blank: Async JS",
            "Pick the missing keyword in the async pattern.",
            """{"sentence":"When using the Fetch API, you must use the ___ keyword before the fetch() call inside an async function to get the resolved value.","options":["then","async","await","resolve"],"category":"JavaScript"}""",
            """{"answer":"await"}""", 10, "Medium", "JavaScript"),

        new(ChallengeType.FillBlank, "Fill the Blank: SQL",
            "Complete the SQL query concept.",
            """{"sentence":"In SQL, a ___ JOIN returns only the rows where there is a match in both tables.","options":["LEFT","RIGHT","INNER","CROSS"],"category":"Databases"}""",
            """{"answer":"INNER"}""", 10, "Easy", "Databases"),

        // ── True / False ───────────────────────────────────────────────────
        new(ChallengeType.TrueFalse, "True or False: JavaScript Basics",
            "Answer 5 quick true/false statements about JavaScript.",
            """{"statements":[{"id":"1","text":"JavaScript is a statically typed language"},{"id":"2","text":"null and undefined are both falsy in JavaScript"},{"id":"3","text":"typeof null returns 'object' in JavaScript"},{"id":"4","text":"JavaScript is single-threaded"},{"id":"5","text":"Arrow functions have their own 'this' binding"}]}""",
            """{"answers":{"1":false,"2":true,"3":true,"4":true,"5":false}}""", 15, "Medium", "JavaScript"),

        new(ChallengeType.TrueFalse, "True or False: React Facts",
            "Test your React knowledge with these true/false questions.",
            """{"statements":[{"id":"1","text":"React is a full MVC framework"},{"id":"2","text":"Virtual DOM helps React minimise real DOM updates"},{"id":"3","text":"useEffect with an empty dependency array runs once on mount"},{"id":"4","text":"React components must return a single root element"},{"id":"5","text":"Props can be modified directly inside a child component"}]}""",
            """{"answers":{"1":false,"2":true,"3":true,"4":true,"5":false}}""", 15, "Medium", "React"),

        new(ChallengeType.TrueFalse, "True or False: Git & Version Control",
            "Quick fire Git true/false questions.",
            """{"statements":[{"id":"1","text":"git pull is equivalent to git fetch + git merge"},{"id":"2","text":"git stash permanently deletes uncommitted changes"},{"id":"3","text":"A git commit can have more than one parent"},{"id":"4","text":"git rebase rewrites commit history"},{"id":"5","text":"git clone downloads only the latest commit"}]}""",
            """{"answers":{"1":true,"2":false,"3":true,"4":true,"5":false}}""", 15, "Medium", "Git"),

        new(ChallengeType.TrueFalse, "True or False: CSS & HTML",
            "Test your CSS and HTML fundamentals.",
            """{"statements":[{"id":"1","text":"CSS specificity: ID selectors outrank class selectors"},{"id":"2","text":"display:none removes an element from the document flow"},{"id":"3","text":"The <div> element is an inline element"},{"id":"4","text":"CSS variables are declared with -- prefix"},{"id":"5","text":"Flexbox and Grid cannot be used together on the same page"}]}""",
            """{"answers":{"1":true,"2":true,"3":false,"4":true,"5":false}}""", 15, "Medium", "CSS"),

        new(ChallengeType.TrueFalse, "True or False: Databases",
            "Test your database knowledge.",
            """{"statements":[{"id":"1","text":"A primary key can contain NULL values"},{"id":"2","text":"An index speeds up SELECT queries but can slow INSERT"},{"id":"3","text":"SQL is case-sensitive for string comparisons by default"},{"id":"4","text":"NoSQL databases cannot support ACID transactions"},{"id":"5","text":"A foreign key ensures referential integrity"}]}""",
            """{"answers":{"1":false,"2":true,"3":false,"4":false,"5":true}}""", 15, "Medium", "Databases"),

        new(ChallengeType.TrueFalse, "True or False: Algorithms",
            "Test your algorithms and complexity knowledge.",
            """{"statements":[{"id":"1","text":"A binary search requires the input array to be sorted"},{"id":"2","text":"Merge sort has O(n log n) average time complexity"},{"id":"3","text":"Bubble sort is the most efficient general-purpose sort"},{"id":"4","text":"A hash table lookup is O(1) on average"},{"id":"5","text":"Depth-first search uses a queue data structure"}]}""",
            """{"answers":{"1":true,"2":true,"3":false,"4":true,"5":false}}""", 15, "Hard", "Algorithms"),

        // ── Code Bug ───────────────────────────────────────────────────────
        new(ChallengeType.CodeBug, "Find the Bug: Wrong Operator",
            "Spot the bug in this JavaScript function.",
            """{"language":"javascript","code":"function add(a, b) {\n  return a - b;\n}","description":"This function should return the sum of two numbers. What is the bug?","options":["Missing return statement","Wrong operator: should be + not -","Incorrect parameter names","Missing semicolons"],"category":"JavaScript"}""",
            """{"correct":1}""", 15, "Easy", "JavaScript"),

        new(ChallengeType.CodeBug, "Find the Bug: Off-by-One",
            "Find the off-by-one error in this loop.",
            """{"language":"javascript","code":"function printItems(arr) {\n  for (let i = 0; i <= arr.length; i++) {\n    console.log(arr[i]);\n  }\n}","description":"This function should print every item in the array but throws an error. What is wrong?","options":["Missing semicolon after loop","Loop condition should be i < arr.length not i <= arr.length","console.log should be console.error","arr should be called items"],"category":"JavaScript"}""",
            """{"correct":1}""", 15, "Medium", "JavaScript"),

        new(ChallengeType.CodeBug, "Find the Bug: Missing Await",
            "Identify the async/await mistake.",
            """{"language":"javascript","code":"async function getUser(id) {\n  const response = fetch(`/api/users/${id}`);\n  const data = response.json();\n  return data;\n}","description":"This async function should fetch user data but returns a Promise instead of data. What is the bug?","options":["fetch is not a valid function","Missing await before fetch() and response.json()","Template literal syntax is wrong","return should not be used in async functions"],"category":"JavaScript"}""",
            """{"correct":1}""", 20, "Medium", "JavaScript"),

        new(ChallengeType.CodeBug, "Find the Bug: Wrong Comparison",
            "Find the comparison operator mistake.",
            """{"language":"javascript","code":"function isAdult(age) {\n  if (age = 18) {\n    return true;\n  }\n  return false;\n}","description":"This function should return true if age is 18 or above. What is the bug?","options":["Missing curly braces","Using = (assignment) instead of >= (comparison)","return true should be return 1","Function name is misleading"],"category":"JavaScript"}""",
            """{"correct":1}""", 15, "Easy", "JavaScript"),

        new(ChallengeType.CodeBug, "Find the Bug: Infinite Loop",
            "Spot why this loop never ends.",
            """{"language":"javascript","code":"let count = 0;\nwhile (count < 10) {\n  console.log(count);\n}","description":"This loop should print numbers 0–9 but runs forever. What is the bug?","options":["while should be for","Missing count++ to increment the counter","console.log should be outside the loop","count should start at 1"],"category":"JavaScript"}""",
            """{"correct":1}""", 15, "Easy", "JavaScript"),

        new(ChallengeType.CodeBug, "Find the Bug: Python Indentation",
            "Find the indentation bug in this Python code.",
            """{"language":"python","code":"def factorial(n):\n    if n == 0:\n        return 1\n    return n * factorial(n - 1)\nresult = factorial(5)\n    print(result)","description":"This Python factorial function is correct but calling it causes a SyntaxError. Why?","options":["factorial should not call itself","Missing colon after def","print(result) is incorrectly indented — it should not be indented","n - 1 should be n + 1"],"category":"Python"}""",
            """{"correct":2}""", 20, "Medium", "Python"),
    };
}
