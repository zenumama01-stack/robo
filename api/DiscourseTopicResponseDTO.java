 * A DTO class mapped to the Discourse topic API.
public class DiscourseTopicResponseDTO {
    @SerializedName("post_stream")
    public DiscoursePostStream postStream;
    @SerializedName("updated_at")
    public Date updatedAt;
    @SerializedName("last_posted")
    public Date lastPosted;
    public DiscourseTopicDetails details;
    public static class DiscoursePostAuthor {
    public static class DiscoursePostLink {
        public String url;
        public Boolean internal;
        public Integer clicks;
    public static class DiscoursePostStream {
        public DiscoursePost[] posts;
    public static class DiscoursePost {
        @SerializedName("display_username")
        public String displayUsername;
        public String cooked;
        @SerializedName("link_counts")
        public DiscoursePostLink[] linkCounts;
    public static class DiscourseTopicDetails {
        @SerializedName("created_by")
        public DiscoursePostAuthor createdBy;
        @SerializedName("last_poster")
        public DiscoursePostAuthor lastPoster;
        public DiscoursePostLink[] links;
