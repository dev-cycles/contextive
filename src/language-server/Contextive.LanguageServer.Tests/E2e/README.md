E2e tests use a TestClient to interact with the public interface of the language server - the LSP protocol itself.

They should be ignorant of internal implementation details and not require mocks.

If particular files or folder structures are required, they should be setup in the 'fixtures' folder.

Refactoring the design should not require changes to these tests.