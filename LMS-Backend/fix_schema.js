const sqlite3 = require('sqlite3').verbose();
const db = new sqlite3.Database('c:/LMS-UPDATED-for-testing-main/LMS-UPDATED-for-testing-main/LMS-Backend/src/LiveExpert.API/liveexpert.db');

db.serialize(() => {
    db.run("ALTER TABLE Users ADD COLUMN PasswordResetToken TEXT", function(err) {
        if (err) console.error("Error adding Token:", err.message);
        else console.log("Added PasswordResetToken");
    });
    db.run("ALTER TABLE Users ADD COLUMN PasswordResetTokenExpiresAt TEXT", function(err) {
        if (err) console.error("Error adding Expiration:", err.message);
        else console.log("Added PasswordResetTokenExpiresAt");
    });
});
