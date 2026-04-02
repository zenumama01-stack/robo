 * Action to retrieve certificates earned by users in LearnWorlds
@RegisterClass(BaseAction, 'GetCertificatesAction')
export class GetCertificatesAction extends LearnWorldsBaseAction {
     * Get certificates for a user or course
            const dateFrom = this.getParamValue(Params, 'DateFrom');
            const dateTo = this.getParamValue(Params, 'DateTo');
            const includeDownloadLinks = this.getParamValue(Params, 'IncludeDownloadLinks') !== false;
            const sortBy = this.getParamValue(Params, 'SortBy') || 'issued_at';
            const sortOrder = this.getParamValue(Params, 'SortOrder') || 'desc';
            // Require either userId or courseId
            if (!userId && !courseId) {
                    Message: 'Either UserID or CourseID is required',
            const queryParams: any = {
                limit: Math.min(maxResults, 100),
                sort: sortBy,
                order: sortOrder
                queryParams.issued_after = new Date(dateFrom).toISOString();
                queryParams.issued_before = new Date(dateTo).toISOString();
            // Determine endpoint based on parameters
            let endpoint = '/certificates';
            if (userId && courseId) {
                // Get specific certificate for user/course combination
                endpoint = `/users/${userId}/courses/${courseId}/certificate`;
            } else if (userId) {
                // Get all certificates for a user
                endpoint = `/users/${userId}/certificates`;
            } else if (courseId) {
                // Get all certificates issued for a course
                endpoint = `/courses/${courseId}/certificates`;
            // Build query string
            const queryString = Object.keys(queryParams).length > 0 
                ? '?' + new URLSearchParams(queryParams).toString()
            // Get certificates
            const certificatesResponse = await this.makeLearnWorldsRequest(
                endpoint + queryString,
            if (!certificatesResponse.success) {
                    Message: certificatesResponse.message || 'Failed to retrieve certificates',
            // Handle single certificate vs array
            const rawData = certificatesResponse.data;
            const certificatesArray = Array.isArray(rawData) 
                ? rawData 
                : (rawData?.data || (rawData?.id ? [rawData] : []));
            const formattedCertificates: any[] = [];
            // Process each certificate
            for (const cert of certificatesArray) {
                const formattedCert: any = {
                    id: cert.id || cert.certificate_id,
                    userId: cert.user_id || userId,
                    courseId: cert.course_id || courseId,
                    certificateNumber: cert.certificate_number || cert.number,
                    issuedAt: cert.issued_at || cert.created_at,
                    expiresAt: cert.expires_at,
                    status: cert.status || 'active',
                    grade: cert.grade,
                    score: cert.score,
                    completionPercentage: cert.completion_percentage || 100
                // Add user info if available
                if (cert.user) {
                    formattedCert.user = {
                        id: cert.user.id || cert.user_id,
                        email: cert.user.email,
                        name: cert.user.name || `${cert.user.first_name || ''} ${cert.user.last_name || ''}`.trim()
                } else if (!userId && cert.user_id) {
                    // Try to get user info
                        `/users/${cert.user_id}`,
                            id: user.id,
                            name: `${user.first_name || ''} ${user.last_name || ''}`.trim() || user.username
                // Add course info if available
                if (cert.course) {
                    formattedCert.course = {
                        id: cert.course.id || cert.course_id,
                        title: cert.course.title,
                        duration: cert.course.duration
                } else if (!courseId && cert.course_id) {
                    // Try to get course info
                        `/courses/${cert.course_id}`,
                        const course = courseResponse.data;
                            id: course.id,
                            title: course.title,
                            duration: course.duration
                // Add download links if requested
                if (includeDownloadLinks) {
                    formattedCert.downloadLinks = {
                        pdf: cert.pdf_url || cert.download_url,
                        image: cert.image_url,
                        publicUrl: cert.public_url || cert.certificate_url
                // Add verification info
                formattedCert.verification = {
                    url: cert.verification_url,
                    code: cert.verification_code,
                    qrCode: cert.qr_code_url
                formattedCertificates.push(formattedCert);
            const totalCertificates = formattedCertificates.length;
            const activeCertificates = formattedCertificates.filter(c => 
                c.status === 'active' && (!c.expiresAt || new Date(c.expiresAt) > new Date())
            const expiredCertificates = formattedCertificates.filter(c => 
                c.expiresAt && new Date(c.expiresAt) <= new Date()
            // Group by course if getting user certificates
            const certificatesByCourse: any = {};
            if (userId && !courseId) {
                formattedCertificates.forEach(cert => {
                    const courseTitle = cert.course?.title || 'Unknown Course';
                    if (!certificatesByCourse[courseTitle]) {
                        certificatesByCourse[courseTitle] = [];
                    certificatesByCourse[courseTitle].push(cert);
            // Group by user if getting course certificates
            const certificatesByUser: any = {};
            if (courseId && !userId) {
                    const userName = cert.user?.name || cert.user?.email || 'Unknown User';
                    if (!certificatesByUser[userName]) {
                        certificatesByUser[userName] = [];
                    certificatesByUser[userName].push(cert);
                totalCertificates: totalCertificates,
                activeCertificates: activeCertificates,
                expiredCertificates: expiredCertificates,
                    from: dateFrom || 'all-time',
                    to: dateTo || 'current'
                filterType: userId && courseId ? 'user-course' : (userId ? 'user' : 'course'),
                groupedData: userId && !courseId ? certificatesByCourse : (courseId && !userId ? certificatesByUser : null)
            const certificatesParam = outputParams.find(p => p.Name === 'Certificates');
            if (certificatesParam) {
                certificatesParam.Value = formattedCertificates;
            const totalCountParam = outputParams.find(p => p.Name === 'TotalCount');
            if (totalCountParam) {
                totalCountParam.Value = totalCertificates;
                Message: `Retrieved ${totalCertificates} certificate(s)`,
                Message: `Error retrieving certificates: ${errorMessage}`,
                Name: 'DateFrom',
                Name: 'DateTo',
                Name: 'IncludeDownloadLinks',
                Value: 'issued_at'
                Value: 'desc'
                Name: 'Certificates',
        return 'Retrieves certificates earned by users in LearnWorlds courses with download links and verification info';
