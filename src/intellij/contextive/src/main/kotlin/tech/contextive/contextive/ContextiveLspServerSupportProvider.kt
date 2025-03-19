package tech.contextive.contextive

import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.platform.lsp.api.LspServerSupportProvider

class ContextiveLspServerSupportProvider : LspServerSupportProvider {
    override fun fileOpened(
        project: Project,
        file: VirtualFile,
        serverStarter: LspServerSupportProvider.LspServerStarter
    ) {
        val contextiveManager = ContextiveManager(
            ContextiveActiveChecker(project),
            LanguageServerDownloadScheduler(
                LanguageServerLocationResolver(),
                LanguageServerDownloader(),
                FileSystem(),
                LspManager()),
            project,
            serverStarter
        )
        contextiveManager.startIfRequired()
    }
}