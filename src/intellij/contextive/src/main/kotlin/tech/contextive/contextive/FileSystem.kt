package tech.contextive.contextive

import java.nio.file.Files
import java.nio.file.Path
import java.nio.file.attribute.PosixFilePermission
import kotlin.io.path.exists
import kotlin.io.path.getPosixFilePermissions
import kotlin.io.path.setPosixFilePermissions

class FileSystem {
    fun exists(path: Path): Boolean = path.exists()

    fun createDirectories(path: Path): Path? = Files.createDirectories(path)
    
    fun setExecutable(path: Path): Path =
        path.setPosixFilePermissions(
            path.getPosixFilePermissions()
                .plus(PosixFilePermission.OWNER_EXECUTE)
        )

}