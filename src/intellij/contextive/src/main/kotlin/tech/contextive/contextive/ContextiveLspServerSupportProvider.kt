package tech.contextive.contextive

import com.intellij.execution.configurations.GeneralCommandLine
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.platform.lsp.api.LspServerSupportProvider
import com.intellij.platform.lsp.api.ProjectWideLspServerDescriptor

class ContextiveLspServerSupportProvider : LspServerSupportProvider {
    override fun fileOpened(project: Project, file: VirtualFile, serverStarter: LspServerSupportProvider.LspServerStarter) {
        serverStarter.ensureServerStarted(ContextiveLspServerDescriptor(project))
    }
}
private class ContextiveLspServerDescriptor(project: Project) : ProjectWideLspServerDescriptor(project, "Contextive") {
    override fun isSupportedFile(file: VirtualFile) = true
    override fun createCommandLine(): GeneralCommandLine = GeneralCommandLine("Contextive.LanguageServer")
}