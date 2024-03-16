package tech.contextive.contextive

import com.intellij.ide.plugins.PluginManagerCore
import com.intellij.openapi.diagnostic.logger
import com.intellij.openapi.extensions.PluginId
import net.lingala.zip4j.io.inputstream.ZipInputStream
import net.lingala.zip4j.model.LocalFileHeader
import java.io.FileOutputStream
import java.net.URI
import java.nio.file.Files
import java.nio.file.Path
import java.nio.file.attribute.PosixFilePermission
import kotlin.io.path.exists
import kotlin.io.path.getPosixFilePermissions
import kotlin.io.path.setPosixFilePermissions


private val LOG = logger<LanguageServerDownloader>()

private const val LANGUAGE_SERVER_TEMPLATE =
    "https://github.com/dev-cycles/contextive/releases/download/v%s/Contextive.LanguageServer-%s-%s-%1\$s.zip"
private const val CONTEXTIVE_ID = "tech.contextive.contextive"

fun getOsCode(): String = System.getProperty("os.name").lowercase().run {
    when {
        "win" in this -> "win"
        "mac" in this -> "osx"
        else -> "linux"
    }
}

private fun getArchCode(): String = System.getProperty("os.arch").lowercase().run {
    when {
        "aarch64" in this -> "arm64"
        else -> "x64"
    }
}

private fun getLanguageServerFileName(): String = "Contextive.LanguageServer" + getOsCode().run {
    when {
        "win" in this -> ".exe"
        else -> ""
    }
}

fun getLsPath(): Path =
    PluginManagerCore
        .getPlugin(PluginId.getId(CONTEXTIVE_ID))!!.run {
            this.pluginPath.resolve("language-server")
                .resolve(this.version).resolve(getLanguageServerFileName())
        }


private fun downloadLanguageServer(languageServerZipUrl: String, lsPath: Path) {
    var localFileHeader: LocalFileHeader?
    val readBuffer = ByteArray(4096)
    var readLen: Int

    var destination = lsPath.parent

    LOG.info("Downloading LanguageServer from $languageServerZipUrl")
    Files.createDirectories(destination)
    val zipInputStream = ZipInputStream(URI(languageServerZipUrl).toURL().openStream())
    while (zipInputStream.nextEntry.also { localFileHeader = it } != null) {
        val extractedFile = destination.resolve(localFileHeader!!.fileName).toFile()
        LOG.info("Extracting `${localFileHeader!!.fileName}` to `${extractedFile.path}`")
        FileOutputStream(extractedFile).use { outputStream ->
            while (zipInputStream.read(readBuffer).also { readLen = it } != -1) {
                outputStream.write(readBuffer, 0, readLen)
            }
        }
    }
    lsPath.setPosixFilePermissions(lsPath.getPosixFilePermissions().plus(PosixFilePermission.OWNER_EXECUTE))
    LOG.info("LanguageServer downloaded and extracted.")
}

fun downloadLanguageServerIfNotFound(): Path {
    val lsPath = getLsPath()
    val plugin = PluginManagerCore.getPlugin(PluginId.getId(CONTEXTIVE_ID))!!
    LOG.info("Looking for LanguageServer at `${lsPath}`")
    if (!lsPath.exists()) {
        val url = LANGUAGE_SERVER_TEMPLATE.format(plugin.version, getOsCode(), getArchCode())
        downloadLanguageServer(url, lsPath)
    } else {
        LOG.info("Found LanguageServer, not downloading.")
    }
    return lsPath
}

class LanguageServerDownloader