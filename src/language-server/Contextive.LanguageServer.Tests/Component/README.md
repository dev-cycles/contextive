Component tests tend to be those that are testing components in isolate.

A component could be a single module, or a collection of modules working together.

In some cases may require passing in mock functions to implement dependencies, such as interaction with the filesystem or with the LSP server.

Refactoring the design may require making changes to these tests.