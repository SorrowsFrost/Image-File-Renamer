✨ Project Story
This project — like all of my public Git experiments — has two purposes:
1. 	To build small, useful utilities that solve real problems for me, my friends, and my family.
2. 	To document the journey of learning programming alongside Copilot, showing how human + AI collaboration can create tools that are practical, transparent, and fun.
I’ve spent over 15 years in FinTech, mostly in support roles. While I never formally trained as a developer, I often had to debug and diagnose data import issues in C‑based interfaces. That experience taught me how to read code and understand what’s happening under the hood.
Now, I’m self‑teaching programming through projects like this. Copilot isn’t here to replace developers — it’s here to augment creativity and speed. For example, in a previous project (a shortcut bar app), Copilot scaffolded the core features quickly, but when we hit a stubborn DPI scaling bug, it couldn’t solve it alone. By combining my debugging observations with Copilot’s reasoning, we cracked the issue together. That’s the essence of this journey: two brains working side by side.

🛠️ What This Tool Does
The Image File Renamer is designed to clean up messy photo collections by:
• 	Reading DateTaken from EXIF metadata.
• 	Renaming files into a consistent timestamp format:

• 	Moving files into a Year/Month folder hierarchy for easy organization.
• 	Handling duplicates gracefully (skip, overwrite, append counter).
• 	Logging every action with clear, import‑friendly delimiters () and summary stats at the top.
This makes it ideal for:
• 	Consolidating family photo archives.
• 	Cleaning up collections after using third‑party apps that caused naming chaos.
• 	Preparing large batches (10k+ files) for long‑term storage or sharing.

📖 Why It Matters
Messy filenames and duplicate chaos are more than just an annoyance — they make it harder to preserve memories. By building this tool, I’m not just solving a technical problem, I’m creating a transparent, scalable way to restore order to personal archives.
The project also demonstrates how AI collaboration can empower someone without formal training to build tools that are:
• 	Transparent: every step logged, every duplicate handled visibly.
• 	Scalable: tested on batches of 12k+ and now 44k+ files.
• 	User‑friendly: clear UI, color‑coded previews, and importable logs.

🚀 Features
• 	EXIF Metadata Parsing: Reads  down to the second.
• 	Batch Renaming: Applies consistent timestamp format.
• 	Folder Organization: Moves files into  hierarchy.
• 	Duplicate Handling: Skip, overwrite, or append counter — with color‑coded previews (grey, red, green).
• 	Audit Logging:
• 	 delimiter for Excel import.
• 	Summary stats at the top.
• 	Elapsed time included for transparency.

📷 Screenshots (to be added)
• 	Preview window showing color‑coded duplicate handling.
• 	Example of Year/Month folder hierarchy.
• 	Sample log file imported into Excel.

===== Image File Renamer Log =====

Total files processed: 44920
Skipped: 120
Overwritten: 300
Appended: 50
Elapsed time: 00:12:34


OriginalName|NewName|Action|Result

IMG_001.jpg|2025-12-05_13-04-22.jpg|Rename|Success

IMG_001.jpg|2025-12-05_13-04-22(1).jpg|Append|Success

IMG_002.jpg|2025-12-05_13-05-10.jpg|Rename|Success

IMG_003.jpg|2025-12-05_13-06-45.jpg|Skip|Duplicate detected

IMG_004.jpg|2025-12-05_13-07-01.jpg|Overwrite|Success


Preview Window (Duplicate Handling)

Grey   | IMG_003.jpg → Skipped (duplicate detected)

Red    | IMG_004.jpg → Overwritten with 2025-12-05_13-07-01.jpg

Green  | IMG_001.jpg → Appended as 2025-12-05_13-04-22(1).jpg

- Grey = Skip (file left untouched, duplicate avoided).
- Red = Overwrite (existing file replaced with new version).
- Green = Append (new file saved with counter suffix to preserve both).




⚙️ Usage
1. 	Select your photo directory.
2. 	Run the renamer.
3. 	Review the preview window for duplicates (color‑coded).
4. 	Confirm and process.
5. 	Check the log file for summary stats and elapsed time.

🧩 Roadmap (Current Status)
• 	✅ Fix noisy extension logging
• 	🔲 Add Excel‑friendly log delimiter ()
• 	🔲 Move summary stats to top of log
• 	🔲 Add elapsed time metric
• 	🔲 Resolve green append color styling
• 	✅ Public GitHub release with documentation
• 	🔲 Add Batch Cancel support with graceful stop + log summary
• 	🔲 Add DateTaken fallback behavior (creation date, modified date, or skip)
• 	🔲 Add Save Source/Destination paths option
• 	🔲 Replace “Show Rules” with Advanced Options and Rules hub
• 	🔲 Add Copy/Move toggle to Advanced Options
• 	🔲 Add Overwrite behavior option (silent overwrite vs prompt)
• 	🔲 Add File Type Skipping (default skip /system files, allow override)
• 	🔲 Add Audit Log verbosity (verbose vs trimmed)
• 	🔲 Add Audit Log destination (file, console, or both — console only in Safety Mode)
• 	🔲 Add Performance ⚡ vs Safety 🛡️ mode toggle
• 	🔲 Add Console view (Safety Mode only, live logging with color coding)
• 	🔲 Ensure Advanced Options screen matches main UI style
• 	🔲 Add Chrome bar dark mode support
• 	🔲 Add friendly logging messages (e.g., “Skipped system file: desktop.ini”)
• 	🔲 Future: integrate with shortcut bar project for unified workflow



🤝 Collaboration
These projects are experiments in human + AI collaboration. If you find improvements, fork it, enhance it, and share back. My goal isn’t to make “perfect” apps — it’s to show how iterative, transparent development with Copilot can empower anyone to build useful tools.

📜 License
MIT License — free to use, modify, and share.
