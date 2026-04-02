package org.openhab.core.addon.marketplace.internal.community.model;
 * A DTO class mapped to the Discourse category topic list API.
public class DiscourseCategoryResponseDTO {
    public DiscourseUser[] users;
    @SerializedName("topic_list")
    public DiscourseTopicList topicList;
    public static class DiscourseUser {
        public Integer id;
        public String username;
        public String name;
        @SerializedName("avatar_template")
        public String avatarTemplate;
    public static class DiscourseTopicList {
        @SerializedName("more_topics_url")
        public String moreTopicsUrl;
        @SerializedName("per_page")
        public Integer perPage;
        public DiscourseTopicItem[] topics;
    public static class DiscoursePosterInfo {
        public String extras;
        public String description;
        @SerializedName("user_id")
        public Integer userId;
    public static class DiscourseTopicItem {
        public String title;
        public String slug;
        public TagDTO[] tags;
        @SerializedName("posts_count")
        public Integer postsCount;
        @SerializedName("image_url")
        public String imageUrl;
        @SerializedName("created_at")
        public Date createdAt;
        @SerializedName("like_count")
        public Integer likeCount;
        public Integer views;
        @SerializedName("category_id")
        public Integer categoryId;
        public DiscoursePosterInfo[] posters;
