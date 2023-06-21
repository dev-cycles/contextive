module Contextive.VsCodeExtension.Tests.E2E.SingleRoot.Paths

let workspace = "../test/single-root/fixtures/simple_workspace"

let inWorkspace path = Node.Api.path.join (workspace, path)
