package tech.contextive.contextive

import ai.grazie.utils.attributes.Attributes
import com.intellij.openapi.application.ApplicationManager
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import java.net.URI
import java.nio.file.Path
import java.util.concurrent.atomic.AtomicBoolean

class LanguageServerDownloadScheduler(val resolver: LanguageServerLocationResolver, val downloader: LanguageServerDownloader, val fileSystem: FileSystem) {

    companion object {
        private val isDownloading = AtomicBoolean()
        fun resetDownloadingStatus() = isDownloading.set(false)
    }

    fun prepAndDownload(url: URI, path: Path) {
        fileSystem.createDirectories(path.parent)
        downloader.download(url, path)
        fileSystem.setExecutable(path)
    }

    fun scheduleDownloadIfRequired(onDownloadComplete: () -> Unit, onAlreadyDownloaded: (Path) -> Unit) {
        val path = resolver.path()
        val url = resolver.url()
        if (isDownloading.get()) {
            return
        }
        if (fileSystem.exists(path)) {
            onAlreadyDownloaded(path)
        } else {
            isDownloading.set(true)
            ApplicationManager.getApplication().executeOnPooledThread {
                prepAndDownload(url, path)
                onDownloadComplete()
                isDownloading.set(false)
            }
        }
    }

}
