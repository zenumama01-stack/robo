import org.eclipse.emf.ecore.EClass;
import org.eclipse.emf.ecore.resource.impl.ResourceImpl;
import org.eclipse.xtext.resource.EObjectDescription;
import org.eclipse.xtext.resource.IEObjectDescription;
import org.eclipse.xtext.scoping.IScope;
import org.eclipse.xtext.scoping.impl.AbstractGlobalScopeProvider;
import org.eclipse.xtext.scoping.impl.SimpleScope;
import com.google.common.base.Predicate;
public class PersistenceGlobalScopeProvider extends AbstractGlobalScopeProvider {
    protected static Resource res = new ResourceImpl();
        res.setURI(URI.createURI("virtual://openhab.org/persistence/strategy.global"));
        res.getContents().add(GlobalStrategies.UPDATE);
        res.getContents().add(GlobalStrategies.CHANGE);
        res.getContents().add(GlobalStrategies.RESTORE);
        res.getContents().add(GlobalStrategies.FORECAST);
    protected IScope getScope(Resource resource, boolean ignoreCase, EClass type,
            Predicate<IEObjectDescription> predicate) {
        IScope parentScope = super.getScope(resource, ignoreCase, type, predicate);
        List<IEObjectDescription> descs = new ArrayList<>();
        for (EObject eObj : res.getContents()) {
            if (eObj instanceof Strategy strategy) {
                descs.add(EObjectDescription.create(strategy.getName(), strategy));
        return new SimpleScope(parentScope, descs);
