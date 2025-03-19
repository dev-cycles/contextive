package tech.contextive.contextive

import org.junit.jupiter.api.Test
import org.junit.jupiter.params.ParameterizedTest
import org.junit.jupiter.params.provider.CsvSource
import kotlin.test.assertEquals

class ContextiveActiveCheckerTest {

    @ParameterizedTest
    @CsvSource("false", "true")
    fun givenContextiveFilePresence_EnsureIsActive(isContextiveFilePresent: Boolean) {
// Arrange
        val project = getMockedProjectWithDefaultGlossary(isContextiveFilePresent)

        val contextiveActiveChecker  = ContextiveActiveChecker(project)

        // Act
        val active = contextiveActiveChecker.isActive()

        // Assert
        assertEquals(active, isContextiveFilePresent)
    }

    /*@Test
    fun givenGlossaryFilesExist_EnsureIsActive() {
        // Arrange
        val project = getMockedProject ()

        val contextiveActiveChecker  = ContextiveActiveChecker(project)

        // Act
        val active = contextiveActiveChecker.isActive()

        // Assert
        assertEquals(active, true)
    }*/
}