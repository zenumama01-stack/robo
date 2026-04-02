 * Action to retrieve comprehensive course details including curriculum structure
@RegisterClass(BaseAction, 'GetLearnWorldsCourseDetailsAction')
export class GetLearnWorldsCourseDetailsAction extends LearnWorldsBaseAction {
     * Get comprehensive details about a specific course including curriculum
            const includeModules = this.getParamValue(Params, 'IncludeModules') !== false;
            const includeInstructors = this.getParamValue(Params, 'IncludeInstructors') !== false;
            const includeStats = this.getParamValue(Params, 'IncludeStats') !== false;
            // Get course details
            if (!courseResponse.success || !courseResponse.data) {
                    Message: courseResponse.message || 'Failed to retrieve course details',
            const courseDetails: any = {
                slug: course.slug,
                description: course.description,
                shortDescription: course.short_description,
                status: course.status || 'published',
                price: course.price || 0,
                originalPrice: course.original_price,
                currency: course.currency || 'USD',
                level: course.level || 'all',
                language: course.language || 'en',
                duration: course.duration,
                durationText: this.formatDuration(course.duration),
                totalEnrollments: course.total_enrollments || 0,
                averageRating: course.average_rating,
                totalRatings: course.total_ratings || 0,
                tags: course.tags || [],
                categories: course.categories || [],
                imageUrl: course.image_url,
                videoUrl: course.video_url,
                certificateEnabled: course.certificate_enabled || false,
                createdAt: course.created_at,
                updatedAt: course.updated_at,
                publishedAt: course.published_at
            // Get curriculum/modules if requested
            if (includeModules) {
                const modulesResponse = await this.makeLearnWorldsRequest(
                    `/courses/${courseId}/sections`,
                if (modulesResponse.success && modulesResponse.data) {
                    courseDetails.modules = this.formatModules(modulesResponse.data.data || modulesResponse.data);
                    courseDetails.totalModules = courseDetails.modules.length;
                    courseDetails.totalLessons = courseDetails.modules.reduce((sum: number, module: any) => 
                        sum + (module.lessons?.length || 0), 0
            // Get instructors if requested
            if (includeInstructors) {
                const instructorsResponse = await this.makeLearnWorldsRequest(
                    `/courses/${courseId}/instructors`,
                if (instructorsResponse.success && instructorsResponse.data) {
                    courseDetails.instructors = this.formatInstructors(instructorsResponse.data.data || instructorsResponse.data);
            // Get additional stats if requested
            if (includeStats) {
                const statsResponse = await this.makeLearnWorldsRequest(
                    `/courses/${courseId}/stats`,
                if (statsResponse.success && statsResponse.data) {
                    courseDetails.stats = {
                        totalEnrollments: statsResponse.data.total_enrollments || courseDetails.totalEnrollments,
                        activeStudents: statsResponse.data.active_students || 0,
                        completionRate: statsResponse.data.completion_rate || 0,
                        averageProgressPercentage: statsResponse.data.average_progress || 0,
                        averageTimeToComplete: statsResponse.data.average_time_to_complete,
                        totalRevenue: statsResponse.data.total_revenue || 0
                courseId: courseDetails.id,
                title: courseDetails.title,
                status: courseDetails.status,
                level: courseDetails.level,
                duration: courseDetails.durationText,
                totalModules: courseDetails.totalModules || 0,
                totalLessons: courseDetails.totalLessons || 0,
                totalEnrollments: courseDetails.totalEnrollments,
                averageRating: courseDetails.averageRating || 0,
                certificateEnabled: courseDetails.certificateEnabled,
                price: courseDetails.price,
                currency: courseDetails.currency
            const courseDetailsParam = outputParams.find(p => p.Name === 'CourseDetails');
            if (courseDetailsParam) {
                courseDetailsParam.Value = courseDetails;
                Message: 'Course details retrieved successfully',
                Message: `Error retrieving course details: ${errorMessage}`,
     * Format course modules/sections data
    private formatModules(modules: any[]): any[] {
            id: module.id,
            title: module.title,
            description: module.description,
            order: module.order || module.position || 0,
            duration: module.duration,
            durationText: this.formatDuration(module.duration),
            totalLessons: module.total_lessons || module.lessons?.length || 0,
                id: lesson.id,
                title: lesson.title,
                type: lesson.type || 'video',
                duration: lesson.duration,
                durationText: this.formatDuration(lesson.duration),
                order: lesson.order || lesson.position || 0,
                isFree: lesson.is_free || false,
                hasVideo: lesson.has_video || false,
                hasQuiz: lesson.has_quiz || false,
                hasAssignment: lesson.has_assignment || false
        })).sort((a, b) => a.order - b.order);
     * Format instructor data
    private formatInstructors(instructors: any[]): any[] {
        return instructors.map(instructor => ({
            id: instructor.id,
            name: instructor.name || `${instructor.first_name || ''} ${instructor.last_name || ''}`.trim(),
            email: instructor.email,
            bio: instructor.bio,
            title: instructor.title,
            imageUrl: instructor.image_url || instructor.avatar_url,
            totalCourses: instructor.total_courses || 0,
            totalStudents: instructor.total_students || 0,
            averageRating: instructor.average_rating || 0
                Name: 'IncludeModules',
                Name: 'IncludeInstructors',
                Name: 'IncludeStats',
                Name: 'CourseDetails',
        return 'Retrieves comprehensive details about a specific LearnWorlds course including curriculum structure';
