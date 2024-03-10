package tech.contextive.contextive

import com.intellij.execution.configurations.GeneralCommandLine
import com.intellij.openapi.diagnostic.logger
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.platform.lsp.api.LspServerSupportProvider
import com.intellij.platform.lsp.api.ProjectWideLspServerDescriptor

private val LOG = logger<ContextiveLspServerSupportProvider>()

class ContextiveLspServerSupportProvider : LspServerSupportProvider {
    override fun fileOpened(
        project: Project,
        file: VirtualFile,
        serverStarter: LspServerSupportProvider.LspServerStarter
    ) {
        serverStarter.ensureServerStarted(ContextiveLspServerDescriptor(project))
    }

}

private class ContextiveLspServerDescriptor(project: Project) : ProjectWideLspServerDescriptor(project, "Contextive") {
    override fun isSupportedFile(file: VirtualFile) = true
    override fun createCommandLine(): GeneralCommandLine =
        downloadLanguageServerIfNotFound().run { GeneralCommandLine(this.toString()) }
}