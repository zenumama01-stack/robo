 * Action to retrieve all course enrollments for a specific user
@RegisterClass(BaseAction, 'GetUserEnrollmentsAction')
export class GetUserEnrollmentsAction extends LearnWorldsBaseAction {
     * Get all enrollments for a user
            const includeExpired = this.getParamValue(Params, 'IncludeExpired') || false;
            const includeCourseDetails = this.getParamValue(Params, 'IncludeCourseDetails') !== false;
            const sortBy = this.getParamValue(Params, 'SortBy') || 'enrolled_at';
            if (!includeExpired) {
                queryParams.hide_expired = true;
            // Get user enrollments
            const enrollmentsData = await this.makeLearnWorldsPaginatedRequest<any>(
            const enrollments = enrollmentsData || [];
            const formattedEnrollments: any[] = [];
            // Process each enrollment
            for (const enrollment of enrollments) {
                const formattedEnrollment: any = {
                    courseId: enrollment.course_id,
                        completedLessons: enrollment.completed_lessons || 0,
                        totalLessons: enrollment.total_lessons || 0,
                        lastAccessedAt: enrollment.last_accessed_at,
                        totalTimeSpent: enrollment.total_time_spent || 0,
                        totalTimeSpentText: this.formatDuration(enrollment.total_time_spent || 0)
                    grade: enrollment.grade,
                    certificateIssuedAt: enrollment.certificate_issued_at,
                    completedAt: enrollment.completed_at
                // Get course details if requested
                if (includeCourseDetails && enrollment.course_id) {
                        const course = await this.makeLearnWorldsRequest<any>(
                            `courses/${enrollment.course_id}`,
                        formattedEnrollment.course = {
                            description: course.short_description || course.description,
                            level: course.level,
                            instructorName: course.instructor_name,
                            certificateEnabled: course.certificate_enabled || false
                        // Log error but continue processing
                        console.warn(`Failed to get course details for ${enrollment.course_id}:`, error);
                formattedEnrollments.push(formattedEnrollment);
            const totalEnrollments = formattedEnrollments.length;
            const activeEnrollments = formattedEnrollments.filter(e => e.status === 'active').length;
            const completedEnrollments = formattedEnrollments.filter(e => 
                e.progress.percentage >= 100 || e.completedAt
            const expiredEnrollments = formattedEnrollments.filter(e => e.status === 'expired').length;
            const averageProgress = totalEnrollments > 0 
                ? formattedEnrollments.reduce((sum, e) => sum + e.progress.percentage, 0) / totalEnrollments 
            const totalTimeSpent = formattedEnrollments.reduce((sum, e) => 
                sum + (e.progress.totalTimeSpent || 0), 0
            const certificatesEarned = formattedEnrollments.filter(e => e.certificateIssuedAt).length;
                totalEnrollments: totalEnrollments,
                activeEnrollments: activeEnrollments,
                completedEnrollments: completedEnrollments,
                expiredEnrollments: expiredEnrollments,
                inProgressEnrollments: activeEnrollments - completedEnrollments,
                averageProgressPercentage: Math.round(averageProgress * 100) / 100,
                totalTimeSpent: totalTimeSpent,
                totalTimeSpentText: this.formatDuration(totalTimeSpent),
                certificatesEarned: certificatesEarned,
                enrollmentsByStatus: {
                    active: activeEnrollments,
                    completed: completedEnrollments,
                    expired: expiredEnrollments
            const enrollmentsParam = outputParams.find(p => p.Name === 'Enrollments');
            if (enrollmentsParam) enrollmentsParam.Value = formattedEnrollments;
            if (totalCountParam) totalCountParam.Value = totalEnrollments;
                Message: `Retrieved ${totalEnrollments} enrollments for user`,
                Message: `Error retrieving enrollments: ${errorMessage}`,
                Name: 'IncludeExpired',
                Name: 'IncludeCourseDetails',
                Value: 'enrolled_at'
                Name: 'Enrollments',
        return 'Retrieves all course enrollments for a specific LearnWorlds user with detailed progress information';
