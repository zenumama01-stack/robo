 * Action to update a user's course progress in LearnWorlds
@RegisterClass(BaseAction, 'UpdateUserProgressAction')
export class UpdateUserProgressAction extends LearnWorldsBaseAction {
     * Update user progress for a course or lesson
            const lessonId = this.getParamValue(Params, 'LessonID');
            const progressPercentage = this.getParamValue(Params, 'ProgressPercentage');
            const completed = this.getParamValue(Params, 'Completed');
            const timeSpent = this.getParamValue(Params, 'TimeSpent');
            const score = this.getParamValue(Params, 'Score');
            const notes = this.getParamValue(Params, 'Notes');
            // Get current enrollment first
            let currentEnrollment: any;
                currentEnrollment = await this.makeLearnWorldsRequest<any>(
                    `users/${userId}/enrollments/${courseId}`,
                    ResultCode: 'NOT_ENROLLED',
                    Message: 'User is not enrolled in this course',
            let updateResult: any = {};
            // Update lesson progress if lessonId is provided
            if (lessonId) {
                const lessonProgressData: any = {
                    completed: completed !== undefined ? completed : false,
                    progress_percentage: progressPercentage
                if (timeSpent !== undefined) {
                    lessonProgressData.time_spent = timeSpent;
                if (score !== undefined) {
                    lessonProgressData.score = score;
                if (notes) {
                    lessonProgressData.notes = notes;
                    const lessonUpdateData = await this.makeLearnWorldsRequest<any>(
                        `users/${userId}/courses/${courseId}/lessons/${lessonId}/progress`,
                        lessonProgressData,
                    updateResult.lessonProgress = lessonUpdateData;
                        ResultCode: 'UPDATE_FAILED',
                        Message: error instanceof Error ? error.message : 'Failed to update lesson progress',
            // Update overall course progress if progressPercentage is provided at course level
            if (progressPercentage !== undefined && !lessonId) {
                const courseProgressData: any = {
                if (completed !== undefined) {
                    courseProgressData.completed = completed;
                    courseProgressData.total_time_spent = (currentEnrollment.total_time_spent || 0) + timeSpent;
                    const courseUpdateData = await this.makeLearnWorldsRequest<any>(
                        `users/${userId}/enrollments/${courseId}/progress`,
                        courseProgressData,
                    updateResult.courseProgress = courseUpdateData;
                        Message: error instanceof Error ? error.message : 'Failed to update course progress',
            // Get updated enrollment details
            let updatedProgress = currentEnrollment;
                updatedProgress = await this.makeLearnWorldsRequest<any>(
                // If we can't get updated details, use current enrollment
                console.warn('Failed to get updated enrollment details:', error);
            // Format progress details
            const progressDetails = {
                lessonId: lessonId,
                previousProgress: {
                    percentage: currentEnrollment.progress_percentage || 0,
                    completedUnits: currentEnrollment.completed_units || 0,
                    totalTimeSpent: currentEnrollment.total_time_spent || 0
                updatedProgress: {
                    percentage: updatedProgress.progress_percentage || 0,
                    completedUnits: updatedProgress.completed_units || 0,
                    totalUnits: updatedProgress.total_units || 0,
                    totalTimeSpent: updatedProgress.total_time_spent || 0,
                    totalTimeSpentText: this.formatDuration(updatedProgress.total_time_spent || 0),
                    lastAccessedAt: updatedProgress.last_accessed_at || new Date().toISOString(),
                    completed: updatedProgress.completed || false,
                    completedAt: updatedProgress.completed_at
                updateType: lessonId ? 'lesson' : 'course',
                updateResult: updateResult
                progressIncreased: (updatedProgress.progress_percentage || 0) > (currentEnrollment.progress_percentage || 0),
                previousPercentage: currentEnrollment.progress_percentage || 0,
                newPercentage: updatedProgress.progress_percentage || 0,
                timeAdded: timeSpent || 0,
                isCompleted: updatedProgress.completed || false,
                updateType: progressDetails.updateType
            const progressDetailsParam = outputParams.find(p => p.Name === 'ProgressDetails');
            if (progressDetailsParam) progressDetailsParam.Value = progressDetails;
                Message: `Successfully updated ${progressDetails.updateType} progress`,
                Message: `Error updating progress: ${errorMessage}`,
                Name: 'LessonID',
                Name: 'ProgressPercentage',
                Name: 'TimeSpent',
                Name: 'Score',
                Name: 'Notes',
                Name: 'ProgressDetails',
        return 'Updates a user\'s progress for a course or specific lesson in LearnWorlds';
