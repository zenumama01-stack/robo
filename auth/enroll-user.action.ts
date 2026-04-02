 * Action to enroll a user in a LearnWorlds course
@RegisterClass(BaseAction, 'EnrollUserAction')
export class EnrollUserAction extends LearnWorldsBaseAction {
     * Enroll a user in a course
    public async InternalRunAction(params: RunActionParams): Promise<ActionResultSimple> {
        this.params = Params;
            const userId = this.getParamValue(Params, 'UserID');
            const courseId = this.getParamValue(Params, 'CourseID');
            const price = this.getParamValue(Params, 'Price') || 0;
            const justification = this.getParamValue(Params, 'Justification') || 'API Enrollment';
            const notifyUser = this.getParamValue(Params, 'NotifyUser') !== false;
            const expiryDate = this.getParamValue(Params, 'ExpiryDate');
            if (!userId) {
                    Message: 'UserID is required',
            if (!courseId) {
                    Message: 'CourseID is required',
            // Prepare enrollment data
            const enrollmentData: any = {
                user_id: userId,
                justification: justification,
                price: price,
                notify_user: notifyUser
                enrollmentData.starts_at = new Date(startDate).toISOString();
            if (expiryDate) {
                enrollmentData.expires_at = new Date(expiryDate).toISOString();
            // Create enrollment
            const enrollmentResponse = await this.makeLearnWorldsRequest(
                `/courses/${courseId}/enrollments`,
                enrollmentData,
            if (!enrollmentResponse.success) {
                    Message: enrollmentResponse.message || 'Failed to enroll user',
            const enrollment = enrollmentResponse.data;
            // Format enrollment details
            const enrollmentDetails = {
                id: enrollment.id,
                userId: enrollment.user_id || userId,
                courseId: enrollment.course_id || courseId,
                enrolledAt: enrollment.enrolled_at || enrollment.created_at,
                startsAt: enrollment.starts_at,
                expiresAt: enrollment.expires_at,
                status: enrollment.status || 'active',
                price: enrollment.price || price,
                progress: {
                    percentage: enrollment.progress_percentage || 0,
                    completedUnits: enrollment.completed_units || 0,
                    totalUnits: enrollment.total_units || 0,
                    lastAccessedAt: enrollment.last_accessed_at
                certificateEligible: enrollment.certificate_eligible || false,
                certificateIssuedAt: enrollment.certificate_issued_at
            // Get course and user details for summary
            let courseTitle = 'Unknown Course';
            let userName = 'Unknown User';
            // Try to get course details
            const courseResponse = await this.makeLearnWorldsRequest(
                `/courses/${courseId}`,
            if (courseResponse.success && courseResponse.data) {
                courseTitle = courseResponse.data.title || courseTitle;
            // Try to get user details
            const userResponse = await this.makeLearnWorldsRequest(
                `/users/${userId}`,
            if (userResponse.success && userResponse.data) {
                const user = userResponse.data;
                userName = user.email || user.username || userName;
                enrollmentId: enrollmentDetails.id,
                userId: userId,
                userName: userName,
                courseTitle: courseTitle,
                enrolledAt: enrollmentDetails.enrolledAt,
                status: enrollmentDetails.status,
                price: enrollmentDetails.price,
                notificationSent: notifyUser
            const enrollmentDetailsParam = outputParams.find(p => p.Name === 'EnrollmentDetails');
            if (enrollmentDetailsParam) {
                enrollmentDetailsParam.Value = enrollmentDetails;
            if (summaryParam) {
                summaryParam.Value = summary;
                Message: `Successfully enrolled user ${userName} in course ${courseTitle}`,
                ResultCode: 'EXECUTION_ERROR',
                Message: `Error enrolling user: ${errorMessage}`,
                Name: 'UserID',
                Name: 'CourseID',
                Name: 'Price',
                Name: 'Justification',
                Value: 'API Enrollment'
                Name: 'NotifyUser',
                Name: 'ExpiryDate',
                Name: 'EnrollmentDetails',
        return 'Enrolls a user in a LearnWorlds course with optional pricing and notification settings';
