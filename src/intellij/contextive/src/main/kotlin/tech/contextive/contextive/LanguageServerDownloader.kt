package tech.contextive.contextive

import com.intellij.openapi.diagnostic.logger
import com.intellij.openapi.project.Project
import com.intellij.platform.ide.progress.withBackgroundProgress
import com.intellij.platform.util.progress.reportSequentialProgress
import kotlinx.coroutines.ensureActive
import net.lingala.zip4j.io.inputstream.ZipInputStream
import net.lingala.zip4j.model.LocalFileHeader
import java.io.File
import java.io.FileOutputStream
import java.net.URI
import java.nio.file.Files
import java.nio.file.Path

private val LOG = logger<LanguageServerDownloader>()

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

        reportSequentialProgress(1) { reporter ->
            reporter.indeterminateStep("Checking Contextive Language Server...")

            val zipInputStream = ZipInputStream(uri.toURL().openStream())
            while (zipInputStream.nextEntry.also { localFileHeader = it } != null) {
                val extractedFile = destination.resolve(localFileHeader!!.fileName).toFile()
                val tempFile = File(extractedFile.path + ".tmp")
                LOG.info("Extracting `${localFileHeader!!.fileName}` to `${extractedFile.path}`")
                reporter.itemStep("Downloading ${localFileHeader!!.fileName}...") {

                    val estimatedSize = 1024 * 1024 * 75
                    reportSequentialProgress(estimatedSize) { innerReporter ->

                        FileOutputStream(tempFile).use { outputStream ->
                            while (zipInputStream.read(readBuffer).also { readLen = it } != -1) {
                                ensureActive()
                                innerReporter.sizedStep(readLen)
                                outputStream.write(readBuffer, 0, readLen)
                            }
                        }
                        Files.move(tempFile.toPath(), extractedFile.toPath())

                    }

                }
            }

        }

        LOG.info("LanguageServer downloaded and extracted.")
    }
}