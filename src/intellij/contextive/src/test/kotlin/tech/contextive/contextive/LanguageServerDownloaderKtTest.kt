package tech.contextive.contextive

import org.junit.jupiter.api.AfterEach
import org.junit.jupiter.api.Assertions.*
import org.junit.jupiter.api.BeforeEach
import org.junit.jupiter.params.ParameterizedTest
import org.junit.jupiter.params.provider.CsvSource

class LanguageServerDownloaderKtTest {

    @BeforeEach
    fun setUp() {
    }

    @AfterEach
    fun tearDown() {
    }

    @ParameterizedTest
    @CsvSource("Windows,win", "MacOs X,osx", "Ubuntu,linux")
    fun shouldGetOsCode(osName: String, expectedOsCode: String) {
        val existingOsName = System.getProperty("os.name")
        System.setProperty("os.name", osName)

        val osCode = getOsCode()

        assertEquals(expectedOsCode, osCode)

        System.setProperty("os.name", existingOsName)
    }
}