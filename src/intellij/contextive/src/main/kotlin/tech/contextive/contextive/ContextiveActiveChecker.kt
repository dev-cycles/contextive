package tech.contextive.contextive

import com.intellij.openapi.project.BaseProjectDirectories.Companion.getBaseDirectories
import com.intellij.openapi.project.Project
import java.nio.file.FileSystems

class ContextiveActiveChecker(private val project: Project) {
    fun isActive(): Boolean {
        val contextiveGlossaryFile =
            project.getBaseDirectories().first().findFileByRelativePath(".contextive/definitions.yml")

        return contextiveGlossaryFile?.exists() == true
    }
}
