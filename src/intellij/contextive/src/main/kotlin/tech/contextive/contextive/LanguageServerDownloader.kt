package tech.contextive.contextive

import com.intellij.openapi.diagnostic.logger
import com.intellij.platform.ide.progress.withBackgroundProgress
import com.intellij.platform.util.progress.*
import com.intellij.openapi.project.Project
import kotlinx.coroutines.ensureActive
import net.lingala.zip4j.io.inputstream.ZipInputStream
import net.lingala.zip4j.model.LocalFileHeader
import java.io.File
import java.io.FileOutputStream
import java.net.URI
import java.nio.file.Files
import java.nio.file.Path

private val LOG = logger<LanguageServerDownloader>()

@Suppress("DialogTitleCapitalization")
class LanguageServerDownloader {
    suspend fun download(project: Project, uri: URI, path: Path) = withBackgroundProgress(
        project,
        title = "Contextive",
        cancellable = true,
    ) {
        var localFileHeader: LocalFileHeader?
        val readBuffer = ByteArray(4096)
        var readLen: Int

        val destination = path.parent

        LOG.info("Downloading LanguageServer from $uri")
        var estimatedSize = 1024.0 * 1024 * 68
        var downloaded = 0.0
        withRawProgressReporter {
            rawProgressReporter?.details("Downloading Contextive Language Server...")
            val zipInputStream = ZipInputStream(uri.toURL().openStream())
            while (zipInputStream.nextEntry.also { localFileHeader = it } != null) {
                val extractedFile = destination.resolve(localFileHeader!!.fileName).toFile()
                val tempFile = File(extractedFile.path + ".tmp")
                LOG.info("Extracting `${localFileHeader!!.fileName}` to `${extractedFile.path}`")

                FileOutputStream(tempFile).use { outputStream ->
                    while (zipInputStream.read(readBuffer).also { readLen = it } != -1) {
                        ensureActive()
                        downloaded += readLen
                        outputStream.write(readBuffer, 0, readLen)
                        rawProgressReporter?.fraction(downloaded / estimatedSize)
                    }
                }

                Files.move(tempFile.toPath(), extractedFile.toPath())
            }
        }
        LOG.info("LanguageServer downloaded and extracted.")
    }
}