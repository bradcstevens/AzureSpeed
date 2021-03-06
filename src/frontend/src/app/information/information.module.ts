import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";

import { InformationRoutingModule } from "./information-routing.module";
import { InformationComponent } from "./information.component";
import {
  AzureAvailabilityZonesComponent,
  AzureBillingMetersComponent,
  AzureEnvironmentsComponent,
  AzureGeographiesComponent,
  AzureRegionsComponent,
  AzureSovereignCloudsComponent,
  AzureVMPricingComponent,
  IpRangeComponent,
  ReferencesComponent
} from "../information";
import { ComponentsModule } from "../shared/components.module";

@NgModule({
  declarations: [
    AzureAvailabilityZonesComponent,
    AzureBillingMetersComponent,
    AzureEnvironmentsComponent,
    AzureGeographiesComponent,
    AzureRegionsComponent,
    AzureSovereignCloudsComponent,
    AzureVMPricingComponent,
    InformationComponent,
    IpRangeComponent,
    ReferencesComponent
  ],
  imports: [CommonModule, InformationRoutingModule, ComponentsModule],
  providers: [],
  bootstrap: [],
  entryComponents: [InformationComponent],
  exports: []
})
export class InformationModule {}
