package tech.contextive.contextive

import com.intellij.openapi.application.ApplicationManager
import java.net.URI
import java.nio.file.Path

class LanguageServerDownloadScheduler(val resolver: LanguageServerLocationResolver, val downloader: LanguageServerDownloader, val fileSystem: FileSystem) {

    fun prepAndDownload(url: URI, path: Path) {
        fileSystem.createDirectories(path)
        downloader.download(url, path)
        fileSystem.setExecutable(path)
    }

    fun scheduleDownloadIfRequired(onDownloaded: (Path) -> Unit) {
        val path = resolver.path()
        val url = resolver.url()
        if (fileSystem.exists(path)) {
            onDownloaded(path)
        } else {
            ApplicationManager.getApplication().executeOnPooledThread {
                prepAndDownload(url, path)
                onDownloaded(path)
            }
        }
    }

}
