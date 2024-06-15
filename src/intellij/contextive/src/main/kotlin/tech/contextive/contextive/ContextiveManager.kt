package tech.contextive.contextive

import com.intellij.openapi.project.BaseProjectDirectories.Companion.getBaseDirectories
import com.intellij.openapi.project.Project
import com.intellij.platform.lsp.api.LspServerSupportProvider
import com.intellij.platform.lsp.api.LspServerManager

class ContextiveManager(
    val lsDownloader: LanguageServerDownloadScheduler,
    val project: Project,
    val serverStarter: LspServerSupportProvider.LspServerStarter
) {
    fun startIfRequired() {
        val contextiveDefinitionsFile =
            project.getBaseDirectories().first().findFileByRelativePath(".contextive/definitions.yml")

        if (contextiveDefinitionsFile?.exists() == false)
            return;

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
