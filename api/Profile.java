package org.openhab.core.thing.profiles;
 * Common ancestor of all profile types.
 * Profiles define the communication flow between the framework and bindings, i.e. how (and if) certain events and
 * commands are forwarded from the framework to the thing handler and vice versa.
 * Profiles are allowed to maintain some transient state internally, i.e. the same instance of a profile will be used
 * per link for all communication so that the temporal dimension can be taken in account.
public interface Profile {
     * Get the {@link ProfileTypeUID} of this profile.
     * @return the UID of the profile type
    ProfileTypeUID getProfileTypeUID();
     * Will be called if an item has changed its state and this information should be forwarded to the binding.
     * @param state the new state
    void onStateUpdateFromItem(State state);
