module Contextive.VsCodeExtension.Tests.MultiRoot.Paths

let workspace = "../test/multi-root/fixtures/multi-root_mono_workspace"

let inWorkspace path = Node.Api.path.join(workspace, path)