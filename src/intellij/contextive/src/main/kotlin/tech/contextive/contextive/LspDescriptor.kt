package tech.contextive.contextive

import com.intellij.execution.configurations.GeneralCommandLine
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.platform.lsp.api.ProjectWideLspServerDescriptor
import java.nio.file.Path

class LspDescriptor(val languageServerPath: Path, project: Project) : ProjectWideLspServerDescriptor(project, "") {
    override fun createCommandLine(): GeneralCommandLine =
        GeneralCommandLine(languageServerPath.toString())

    override fun isSupportedFile(file: VirtualFile): Boolean = true
}