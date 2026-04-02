package org.openhab.core.karaf.internal.jaas;
import org.apache.karaf.jaas.boot.principal.GroupPrincipal;
import org.apache.karaf.jaas.boot.principal.RolePrincipal;
import org.apache.karaf.jaas.boot.principal.UserPrincipal;
import org.apache.karaf.jaas.modules.BackingEngine;
 * A Karaf backing engine for the {@link UserRegistry}
public class ManagedUserBackingEngine implements BackingEngine {
    public ManagedUserBackingEngine(UserRegistry userRegistry) {
    public void addUser(String username, String password) {
        userRegistry.register(username, password, new HashSet<>(Set.of(Role.USER)));
    public void deleteUser(String username) {
        userRegistry.remove(username);
    public List<UserPrincipal> listUsers() {
        return userRegistry.getAll().stream().map(u -> new UserPrincipal(u.getName())).toList();
    public UserPrincipal lookupUser(String username) {
        User user = userRegistry.get(username);
            return new UserPrincipal(user.getName());
    public List<GroupPrincipal> listGroups(UserPrincipal user) {
    public Map<GroupPrincipal, String> listGroups() {
    public void addGroup(String username, String group) {
    public void createGroup(String group) {
    public void deleteGroup(String username, String group) {
    public List<RolePrincipal> listRoles(Principal principal) {
        User user = userRegistry.get(principal.getName());
            return user.getRoles().stream().map(r -> new RolePrincipal(r)).toList();
    public void addRole(String username, String role) {
        if (user instanceof ManagedUser managedUser) {
            managedUser.getRoles().add(role);
            userRegistry.update(managedUser);
    public void deleteRole(String username, String role) {
            managedUser.getRoles().remove(role);
    public void addGroupRole(String group, String role) {
    public void deleteGroupRole(String group, String role) {
