# CLAUDE.md — Instructions for this repo

This repo is a **60-day C#/.NET backend interview preparation course**. Read this file fully before doing anything in this repo.

## About the learner

- Has **3 years of experience**, but wants to be ready for questions at **5-years-of-experience depth**.
- Primary language: **C# / .NET**. Scope is **backend only**: no frontend (React, Angular, etc.).
- **DSA, HLD and LLD are covered in other repos. Do not teach them here.** Design patterns appear only where they are .NET-specific (Options, Decorator via DI, thread-safe Singleton with `Lazy<T>`, Repository/Unit of Work with EF Core).
- The syllabus is final. **Do not add new chapters** unless a real interview exposes a gap.

## Who the notes are for

Write every note so that **a person with zero knowledge of the topic can read it and become interview-ready**. Assume the reader knows nothing.

- **Plain, simple language.** Short sentences. No unexplained jargon.
- **Define every technical term the first time it appears.**
- Use a real-world analogy when a concept is abstract.
- Go from "what is it" → "why does it exist" → "how it works" → "how to use it" → "what goes wrong" → "what interviewers ask".
- Each `notes.md` must be **self-contained**: the reader should never have to open another file to understand it.

## Real-world examples only

- **Every code example must use a real backend or business scenario**: orders, payments, invoices, carts, shipping, notifications, reports, file storage, repositories, API clients, tax, coupons, inventory, users and roles, and so on.
- **Never use generic textbook examples** like Animal/Dog/Cat, Bird/Penguin, Shape/Circle/Square, Foo/Bar, Base/Derived, A/B/C. The learner must be able to retell each example in an interview as something they built or saw at work.
- Where a textbook example is famous and interviewers expect you to know it (e.g. Square/Rectangle for LSP), mention it briefly in prose, but the code still uses a real scenario.
- Prefer examples that show a **realistic bug or production consequence** (e.g. "a hidden method made PDFs save as .txt"). These make strong interview stories.

## Diagrams

- **Use Mermaid diagrams** (` ```mermaid ` code blocks) wherever a picture makes a concept easier: memory layout, object relationships (class diagrams), execution order (sequence diagrams), request flows, decision trees (flowcharts), state changes.
- Keep diagrams small and focused: one idea per diagram. Add one sentence explaining what the diagram shows.
- Don't add a diagram where it adds nothing.

## Code rules

- **Full code goes inside the notes**, not just a reference to a `.cs` file. The reader must never need to open a `.cs` file while reading a chapter.
- The **same code also exists as a runnable project** in that chapter's `code/` folder, so it can be run with `dotnet run`.
- Show the **expected output** right after each code sample.
- Code must compile and run. Build and run it before marking a chapter done.
- Target **.NET 10 / C# 14** (SDKs installed: .NET 10.0.302 and .NET 8.0.400). When a feature is newer than .NET 8, say which version introduced it, because many employers still run .NET 6/8.
- Library licensing: MediatR and AutoMapper moved to commercial licenses in 2025. Mention this and the alternatives (manual mapping/Mapster, plain handler interfaces) wherever they come up.

## Format for each topic inside notes.md

Each topic heading in a chapter's `notes.md` starts as a placeholder. When the chapter is taught, replace the placeholder with:

1. **What it is**: plain-English explanation (plus an analogy if useful)
2. **Why it matters**: the problem it solves
3. **How it works**: the mechanism, with diagrams in text or tables where helpful
4. **Code**: full, runnable code with comments and the expected output
5. **Common mistakes / gotchas**
6. **5-YOE depth**: the deeper "why" and edge cases senior interviewers probe
7. **Interview questions**: 3–6 likely questions, each with a short model answer

End each chapter's notes with a **Quick revision** section: a one-page cheat sheet of the chapter.

## Teaching workflow (whole chapter at once)

When the learner says "start chapter N" (or similar):

1. Write the **full chapter** in `chapters/NN-.../notes.md`, filling in every topic placeholder using the format above.
2. Create the runnable code in `chapters/NN-.../code/` and verify that it builds and runs.
3. Create `quiz.md` (questions only), `quiz-answers.md` (answers) and `exercises.md` (coding tasks plus 5-YOE scenario questions) in the chapter folder.
4. Tell the learner the chapter is ready to read. Do **not** quiz them until they say they have finished reading.
5. Then run the **chapter test** (see below).
6. Update the status at the top of `notes.md` and the progress table in `README.md`.

## Chapter test (after every chapter)

All four types, in this order:

1. **Live quiz in chat**: ask questions **one at a time**, wait for the answer, grade it (correct / partial / wrong), explain what was missing, then ask the next one. Mix easy and 5-YOE-depth questions.
2. **Coding exercises**: from `exercises.md`. Review the learner's code honestly.
3. **Scenario questions**: interview-style "what would you do if…" questions at 5-YOE depth.
4. The **written quiz file** stays in the repo for later self-testing and revision.

Record every wrong or partial answer in `MISTAKES.md` (chapter, question, what was missed, the correct idea). Weak areas get revisited in Chapters 27–28 and during weekend revision.

## Project: Order Management API

- It lives in `project/OrderManagementApi/` and grows as the chapters progress.
- Milestones and their days are listed in `README.md`.
- Two required extras: a **custom action filter** (Ch 8), and **one read endpoint built with both Dapper and EF Core** for comparison (Ch 16).
- The project should also show .NET-specific patterns (Options, Decorator via DI) so the learner has real examples to talk about in interviews.

## Repo structure

```
README.md            Syllabus overview, schedule, progress tracker, project milestones
CLAUDE.md            This file
MISTAKES.md          Mistake log from quizzes
chapters/NN-name/
  notes.md           Full chapter notes (starts as a topic list, filled in when taught)
  quiz.md            Created when the chapter is taught
  quiz-answers.md    Created when the chapter is taught
  exercises.md       Created when the chapter is taught
  code/              Runnable .NET project(s) for the chapter
project/OrderManagementApi/
```

## Other rules

- Don't commit to git unless the learner asks.
- If the learner gives a new standing instruction, add it to this file.
