package tech.contextive.contextive

import com.intellij.notification.NotificationGroupManager
import com.intellij.notification.NotificationType
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.project.Project
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.runBlocking
import java.net.URI
import java.nio.file.Path
import java.util.concurrent.atomic.AtomicBoolean

open class DownloadStatus {
    class DOWNLOADED(val path: Path) : DownloadStatus()
    class SCHEDULED : DownloadStatus()
    class DOWNLOADING : DownloadStatus()
}

class LanguageServerDownloadScheduler(
    val resolver: LanguageServerLocationResolver,
    val downloader: LanguageServerDownloader,
    val fileSystem: FileSystem,
    val lspManager: LspManager
) {

    private val DOWNLOAD_CANCELLED_MSG = "You have cancelled the Contextive Language Server download.\n\nDownload won't be re-attempted until the IDE is restarted."

    companion object {
        private val isDownloading = AtomicBoolean()
        fun resetDownloadingStatus() = isDownloading.set(false)
    }

    suspend fun prepAndDownload(project: Project, url: URI, path: Path) {
        fileSystem.createDirectories(path.parent)
        downloader.download(project, url, path)
        fileSystem.setExecutable(path)
    }

    fun scheduleDownloadIfRequired(project: Project): DownloadStatus {
        val path = resolver.path()
        val url = resolver.url()
        if (isDownloading.get()) {
            return DownloadStatus.DOWNLOADING()
        }
        if (fileSystem.exists(path)) {
            return DownloadStatus.DOWNLOADED(path)
        } else {
            isDownloading.set(true)
            ApplicationManager.getApplication().executeOnPooledThread {
                runBlocking {
                    try {
                        prepAndDownload(project, url, path)
                        lspManager.start(project, ContextiveLspServerSupportProvider::class.java)
                        isDownloading.set(false)
                    } catch(_: CancellationException) {
                        NotificationGroupManager.getInstance()
                            .getNotificationGroup("Contextive")
                            .createNotification(DOWNLOAD_CANCELLED_MSG, NotificationType.WARNING)
                            .notify(project);
                    }
                }
            }
            return DownloadStatus.SCHEDULED()
        }
    }

}
