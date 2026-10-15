# Copilot Instructions

## Project Guidelines
- In Kiyote.Buffers, prefer inlining single-line guard helpers (e.g. call ObjectDisposedException.ThrowIf( _disposed, this ) directly) rather than wrapping them in private methods like ThrowIfDisposed().
