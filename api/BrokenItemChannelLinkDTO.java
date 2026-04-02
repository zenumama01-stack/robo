package org.openhab.core.io.rest.core.link;
 * Transfer object for broken item channel links.
@Schema(name = "BrokenItemChannelLink")
public class BrokenItemChannelLinkDTO {
    public EnrichedItemChannelLinkDTO itemChannelLink;
    public ItemChannelLinkProblem problem;
    public BrokenItemChannelLinkDTO(EnrichedItemChannelLinkDTO itemChannelLink, ItemChannelLinkProblem problem) {
        this.itemChannelLink = itemChannelLink;
        this.problem = problem;
