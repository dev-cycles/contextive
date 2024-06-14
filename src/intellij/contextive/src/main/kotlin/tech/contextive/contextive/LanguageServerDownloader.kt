package tech.contextive.contextive

import com.intellij.openapi.diagnostic.logger
import net.lingala.zip4j.io.inputstream.ZipInputStream
import net.lingala.zip4j.model.LocalFileHeader
import java.io.FileOutputStream
import java.net.URI
import java.nio.file.Path

private val LOG = logger<LanguageServerDownloader>()

class LanguageServerDownloader {
    fun download(uri: URI, path: Path): Unit {
        var localFileHeader: LocalFileHeader?
        val readBuffer = ByteArray(4096)
        var readLen: Int

        val destination = path.parent

        LOG.info("Downloading LanguageServer from $uri")
        val zipInputStream = ZipInputStream(uri.toURL().openStream())
        while (zipInputStream.nextEntry.also { localFileHeader = it } != null) {
            val extractedFile = destination.resolve(localFileHeader!!.fileName).toFile()
            LOG.info("Extracting `${localFileHeader!!.fileName}` to `${extractedFile.path}`")
            FileOutputStream(extractedFile).use { outputStream ->
                while (zipInputStream.read(readBuffer).also { readLen = it } != -1) {
                    outputStream.write(readBuffer, 0, readLen)
                }
            }
        }
        LOG.info("LanguageServer downloaded and extracted.")
    }
}