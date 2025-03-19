package tech.contextive.contextive

import com.intellij.openapi.project.BaseProjectDirectories
import com.intellij.openapi.project.BaseProjectDirectories.Companion.getBaseDirectories
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import io.mockk.every
import io.mockk.mockk


fun getMockedProject(): Project {
    val baseDirectory = mockk<VirtualFile>(relaxed = true)

    return mockk<Project> {
        every { getService<BaseProjectDirectories>(any()) } returns
                mockk {
                    every { getBaseDirectories() } returns setOf(baseDirectory)
                }
    }
}

fun getMockedProjectWithDefaultGlossary(isContextiveFilePresent: Boolean): Project {
    val baseDirectory = mockk<VirtualFile> {
        every { findFileByRelativePath(".contextive/definitions.yml") } returns
                mockk {
                    every { exists() } returns isContextiveFilePresent
                }
    }

    return mockk<Project> {
        every { getService<BaseProjectDirectories>(any()) } returns
                mockk {
                    every { getBaseDirectories() } returns setOf(baseDirectory)
                }
    }
}

fun getMockedProjectWithGlossaryFiles(files: Array<String>): Project {
    return mockk<Project>()
}