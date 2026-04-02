package org.openhab.core.io.rest.core.fileformat;
 * This is a data transfer object to serialize the different components that can be contained
 * in a file format (items, things, ...) including an optional list of warnings.
@Schema(name = "ExtendedFileFormat")
public class ExtendedFileFormatDTO extends FileFormatDTO {
    @Schema(requiredMode = Schema.RequiredMode.NOT_REQUIRED)
    public List<String> warnings;
