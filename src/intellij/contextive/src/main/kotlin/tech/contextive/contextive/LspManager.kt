package tech.contextive.contextive

import com.intellij.openapi.project.Project
import com.intellij.platform.lsp.api.LspServerManager
import com.intellij.platform.lsp.api.LspServerSupportProvider

class LspManager {
    fun start(project: Project, cls: Class<out LspServerSupportProvider>) {
        LspServerManager.getInstance(project)
            .startServersIfNeeded(cls)
    }
}