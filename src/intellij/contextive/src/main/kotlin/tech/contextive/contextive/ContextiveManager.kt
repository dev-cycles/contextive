package tech.contextive.contextive

import com.intellij.openapi.project.BaseProjectDirectories.Companion.getBaseDirectories
import com.intellij.openapi.project.Project
import com.intellij.platform.lsp.api.LspServerSupportProvider

class ContextiveManager(
    private val lsDownloader: LanguageServerDownloadScheduler,
    val project: Project,
    private val serverStarter: LspServerSupportProvider.LspServerStarter
) {

    fun startIfRequired() {
        val status = lsDownloader.scheduleDownloadIfRequired(project)

        if (status is DownloadStatus.DOWNLOADED) {
            serverStarter.ensureServerStarted(
                LspDescriptor(
                    status.path,
                    project)
            )
        }

    }

}
