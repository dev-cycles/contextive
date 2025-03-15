package tech.contextive.contextive

import com.intellij.openapi.vfs.VirtualFile
import io.mockk.mockk
import org.junit.jupiter.api.Assertions.assertEquals
import org.junit.jupiter.api.Assertions.assertTrue
import org.junit.jupiter.api.Test
import java.nio.file.Path
import java.util.*

class LspDescriptorTest {

    @Test
    fun shouldSupportAllFiles()
    {
        // Arrange
        val descriptor = LspDescriptor(
            Path.of(""),
            getMockedProject(true)
        )

        // Act
        val isFileSupported = descriptor.isSupportedFile(mockk<VirtualFile>(relaxed = true))

        // Assert
        assertTrue(isFileSupported)
    }

    @Test
    fun shouldCreateCommandLineFromPath()
    {
        // Arrange
        val path = Path.of("/" + UUID.randomUUID())
        val descriptor = LspDescriptor(
            path,
            getMockedProject(true)
        )

        // Act
        val command = descriptor.createCommandLine()

        // Assert
        assertEquals(path.toString(), command.commandLineString)
    }
}