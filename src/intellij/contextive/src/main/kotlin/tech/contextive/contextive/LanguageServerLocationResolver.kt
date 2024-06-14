package tech.contextive.contextive

import com.intellij.ide.plugins.PluginManagerCore
import com.intellij.openapi.extensions.PluginId
import java.net.URI
import java.nio.file.Path

private const val LANGUAGE_SERVER_TEMPLATE =
    "https://github.com/dev-cycles/contextive/releases/download/v%s/Contextive.LanguageServer-%s-%s-%1\$s.zip"
private const val CONTEXTIVE_ID = "tech.contextive.contextive"

class LanguageServerLocationResolver {

    private fun getOsCode(): String = System.getProperty("os.name").lowercase().run {
        when {
            "win" in this -> "win"
            "mac" in this -> "osx"
            else -> "linux"
        }
    }

    private fun getArchCode(): String = System.getProperty("os.arch").lowercase().run {
        when {
            "arch64" in this -> "arm64"
            else -> "x64"
        }
    }

    private fun getLanguageServerFileName(): String = "Contextive.LanguageServer" + getOsCode()
        .run {
        when {
            "win" in this -> ".exe"
            else -> ""
        }
    }

    fun url(): URI {
        val plugin = PluginManagerCore.getPlugin(PluginId.getId(CONTEXTIVE_ID))!!
        return URI(LANGUAGE_SERVER_TEMPLATE.format(plugin.version, getOsCode(), getArchCode()))
    }

    fun path(): Path =
        PluginManagerCore
            .getPlugin(PluginId.getId(CONTEXTIVE_ID))!!.run {
                this.pluginPath.resolve("language-server")
                    .resolve(this.version).resolve(getLanguageServerFileName())
            }

}