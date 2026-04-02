import javax.annotation.security.DenyAll;
import javax.annotation.security.PermitAll;
import javax.ws.rs.container.DynamicFeature;
import javax.ws.rs.container.ResourceInfo;
import javax.ws.rs.core.FeatureContext;
 * A {@link DynamicFeature} supporting the {@code javax.annotation.security.RolesAllowed},
 * {@code javax.annotation.security.PermitAll} and {@code javax.annotation.security.DenyAll}
 * on resource methods and sub-resource methods.
 * Ported from the Jersey {@code RolesAllowedDynamicFeature} class with modifications.
 * @author Paul Sandoz - initial contribution
 * @author Martin Matula - initial contribution
 * @author Yannick Schaus - port to openHAB with modifications
public class RolesAllowedDynamicFeatureImpl implements DynamicFeature {
    private final Logger logger = LoggerFactory.getLogger(RolesAllowedDynamicFeatureImpl.class);
    public void configure(ResourceInfo resourceInfo, FeatureContext configuration) {
        final Method am = resourceInfo.getResourceMethod();
            // DenyAll on the method take precedence over RolesAllowed and PermitAll
            if (am.isAnnotationPresent(DenyAll.class)) {
                configuration.register(new RolesAllowedRequestFilter());
            // RolesAllowed on the method takes precedence over PermitAll
            Optional<Annotation> ra = Arrays.stream(am.getAnnotations())
                    .filter(a -> a.annotationType().getName().equals(RolesAllowed.class.getName())).findFirst();
            if (ra.isPresent()) {
                configuration.register(new RolesAllowedRequestFilter(((RolesAllowed) ra.get()).value()));
            // PermitAll takes precedence over RolesAllowed on the class
            if (am.isAnnotationPresent(PermitAll.class)) {
            // DenyAll can't be attached to classes
            // RolesAllowed on the class takes precedence over PermitAll
            ra = Arrays.stream(resourceInfo.getResourceClass().getAnnotations())
            logger.error("Error while configuring the roles", e);
    @Priority(Priorities.AUTHORIZATION) // authorization filter - should go after any authentication filters
    private static class RolesAllowedRequestFilter implements ContainerRequestFilter {
        private final boolean denyAll;
        private final String[] rolesAllowed;
        RolesAllowedRequestFilter() {
            this.denyAll = true;
            this.rolesAllowed = null;
        RolesAllowedRequestFilter(final String[] rolesAllowed) {
            this.denyAll = false;
            this.rolesAllowed = (rolesAllowed != null) ? rolesAllowed : new String[] {};
        public void filter(final ContainerRequestContext requestContext) throws IOException {
            if (!denyAll) {
                if (rolesAllowed.length == 0) {
                for (final String role : rolesAllowed) {
                    if (requestContext.getSecurityContext().isUserInRole(role)) {
            if (!isAuthenticated(requestContext)) {
                        .abortWith(JSONResponse.createErrorResponse(Status.UNAUTHORIZED, "Authentication required"));
            requestContext.abortWith(JSONResponse.createErrorResponse(Status.FORBIDDEN, "Access denied"));
        private static boolean isAuthenticated(final ContainerRequestContext requestContext) {
            return requestContext.getSecurityContext().getUserPrincipal() != null;
