---
name: conventional-commits
description: Use when writing or reviewing commit messages for this repository; apply the Conventional Commits format.
---

# Conventional Commits

Write commit messages in this format:

```text
<type>[optional scope][!]: <description>

[optional body]

[optional footer(s)]
```

- Use a lowercase type such as `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, or `revert`.
- Add a scope only when it clearly identifies the affected area, for example `fix(sync): handle empty responses`.
- Keep the description concise, imperative, and free of a trailing period.
- Use a body to explain why a change was made when the subject is not enough; wrap breaking changes with `!` and describe them in a `BREAKING CHANGE:` footer.
- Do not claim a change is breaking unless it affects compatibility, and do not invent a scope or footer.
- When asked to create a commit, summarize the actual changes in the message and follow the repository's commit requirements.